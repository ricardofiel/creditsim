using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using CreditSim.Core.Models;
using CreditSim.Core.Services;
using CreditSim.Data.Repositories;
using CreditSim.Web.Models;

namespace CreditSim.Web.Controllers
{
    /// <summary>
    /// Web API 2 controller that mirrors all routes in src/routes/simulation.js.
    /// </summary>
    [RoutePrefix("api")]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class SimulationController : ApiController
    {
        private readonly ICreditScoringService _creditScoringService;
        private readonly ICustomerRepository _customerRepository;

        /// <summary>
        /// Parameterless constructor used by IIS Express / Web API infrastructure.
        /// Creates dependencies from Web.config connection string.
        /// </summary>
        public SimulationController()
            : this(
                new CreditScoringService(),
                new CustomerRepository(ConnectionStringProvider.Get())) { }

        /// <summary>Constructor used in unit tests for dependency injection.</summary>
        public SimulationController(
            ICreditScoringService creditScoringService,
            ICustomerRepository customerRepository)
        {
            _creditScoringService = creditScoringService;
            _customerRepository = customerRepository;
        }

        // ------------------------------------------------------------------
        // POST /api/simulate
        // Mirrors: router.post('/simulate', ...) in simulation.js
        // ------------------------------------------------------------------
        [HttpPost, Route("simulate")]
        public async Task<IHttpActionResult> Simulate([FromBody] SimulationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponse
                {
                    Error = "Validation failed",
                    Details = ModelState
                        .Where(kv => kv.Value.Errors.Count > 0)
                        .SelectMany(kv => kv.Value.Errors.Select(e => new
                        {
                            field = kv.Key,
                            message = e.ErrorMessage
                        }))
                        .ToList<object>()
                });
            }

            try
            {
                var result = _creditScoringService.CalculateCreditScore(request);

                var customer = new Customer
                {
                    Name = request.Name,
                    Age = request.Age,
                    AnnualIncome = request.AnnualIncome,
                    DebtToIncomeRatio = request.DebtToIncomeRatio,
                    LoanAmount = request.LoanAmount,
                    CreditHistory = request.CreditHistory,
                    Score = result.Score,
                    RiskCategory = result.RiskCategory
                };

                var saved = await _customerRepository.InsertAsync(customer);

                return Content(HttpStatusCode.Created, new SimulateResponse
                {
                    Id = saved.Id,
                    Score = result.Score,
                    RiskCategory = result.RiskCategory,
                    Message = "Credit score calculated successfully",
                    Customer = new SimulateCustomerDto
                    {
                        Name = request.Name,
                        Age = request.Age,
                        AnnualIncome = request.AnnualIncome,
                        DebtToIncomeRatio = request.DebtToIncomeRatio,
                        LoanAmount = request.LoanAmount,
                        CreditHistory = request.CreditHistory
                    }
                });
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(typeof(SimulationController))
                    .Error("Error in POST /api/simulate", ex);
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Error = "Failed to calculate credit score",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/simulations
        // Mirrors: router.get('/simulations', ...) in simulation.js
        // ------------------------------------------------------------------
        [HttpGet, Route("simulations")]
        public async Task<IHttpActionResult> GetSimulations()
        {
            try
            {
                var simulations = (await _customerRepository.GetAllAsync()).ToList();

                return Ok(new SimulationsListResponse
                {
                    Count = simulations.Count,
                    Simulations = simulations.Select(s => new SimulationSummaryDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Score = s.Score,
                        RiskCategory = s.RiskCategory,
                        LoanAmount = s.LoanAmount,
                        CreatedAt = s.CreatedAt.ToString("o")
                    })
                });
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(typeof(SimulationController))
                    .Error("Error in GET /api/simulations", ex);
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch simulations",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/simulation/{id}
        // Mirrors: router.get('/simulation/:id', ...) in simulation.js
        // ------------------------------------------------------------------
        [HttpGet, Route("simulation/{id:int}")]
        public async Task<IHttpActionResult> GetSimulation(int id)
        {
            if (id < 1)
            {
                return Content(HttpStatusCode.BadRequest, new ErrorResponse
                {
                    Error = "Validation failed",
                    Details = new[] { new { field = "id", message = "ID must be a positive integer" } }
                });
            }

            try
            {
                var simulation = await _customerRepository.GetByIdAsync(id);

                if (simulation == null)
                {
                    return Content(HttpStatusCode.NotFound, new ErrorResponse
                    {
                        Error = "Simulation not found",
                        Message = $"No simulation found with ID {id}"
                    });
                }

                return Ok(new SimulationDetailResponse
                {
                    Simulation = new SimulationDetailDto
                    {
                        Id = simulation.Id,
                        Name = simulation.Name,
                        Age = simulation.Age,
                        AnnualIncome = simulation.AnnualIncome,
                        DebtToIncomeRatio = simulation.DebtToIncomeRatio,
                        LoanAmount = simulation.LoanAmount,
                        CreditHistory = simulation.CreditHistory,
                        Score = simulation.Score,
                        RiskCategory = simulation.RiskCategory,
                        CreatedAt = simulation.CreatedAt.ToString("o")
                    }
                });
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(typeof(SimulationController))
                    .Error($"Error in GET /api/simulation/{id}", ex);
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch simulation",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/scoring-criteria
        // Mirrors: router.get('/scoring-criteria', ...) in simulation.js
        // ------------------------------------------------------------------
        [HttpGet, Route("scoring-criteria")]
        public IHttpActionResult GetScoringCriteria()
        {
            try
            {
                var criteria = _creditScoringService.GetScoringCriteria();
                return Ok(new
                {
                    criteria,
                    disclaimer = "This is a demonstration scoring model and should not be used for actual credit decisions."
                });
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger(typeof(SimulationController))
                    .Error("Error in GET /api/scoring-criteria", ex);
                return Content(HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch scoring criteria",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/health
        // Mirrors: router.get('/health', ...) in simulation.js
        // ------------------------------------------------------------------
        [HttpGet, Route("health")]
        public IHttpActionResult Health()
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var uptime = (DateTime.UtcNow - process.StartTime.ToUniversalTime()).TotalSeconds;

            return Ok(new HealthResponse
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow.ToString("o"),
                Uptime = uptime
            });
        }
    }
}

