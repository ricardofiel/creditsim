using CreditSim.Core.Models;
using CreditSim.Core.Services;
using CreditSim.Data.Repositories;
using CreditSim.Web.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace CreditSim.Web.Controllers
{
    /// <summary>
    /// ASP.NET Core controller that mirrors all routes in src/routes/simulation.js
    /// (and the original Web API 2 controller it replaces).
    /// </summary>
    [ApiController]
    [Route("api")]
    [EnableCors]
    public class SimulationController : ControllerBase
    {
        private readonly ICreditScoringService _creditScoringService;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<SimulationController> _logger;

        public SimulationController(
            ICreditScoringService creditScoringService,
            ICustomerRepository customerRepository,
            ILogger<SimulationController> logger)
        {
            _creditScoringService = creditScoringService;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        // ------------------------------------------------------------------
        // POST /api/simulate
        // ------------------------------------------------------------------
        [HttpPost("simulate")]
        public async Task<IActionResult> Simulate([FromBody] SimulationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse
                {
                    Error = "Validation failed",
                    Details = ModelState
                        .Where(kv => kv.Value is { Errors.Count: > 0 })
                        .SelectMany(kv => kv.Value!.Errors.Select(e => new
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

                return StatusCode(StatusCodes.Status201Created, new SimulateResponse
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
                _logger.LogError(ex, "Error in POST /api/simulate");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "Failed to calculate credit score",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/simulations
        // ------------------------------------------------------------------
        [HttpGet("simulations")]
        public async Task<IActionResult> GetSimulations()
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
                _logger.LogError(ex, "Error in GET /api/simulations");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch simulations",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/simulation/{id}
        // ------------------------------------------------------------------
        [HttpGet("simulation/{id:int}")]
        public async Task<IActionResult> GetSimulation(int id)
        {
            if (id < 1)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponse
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
                    return StatusCode(StatusCodes.Status404NotFound, new ErrorResponse
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
                _logger.LogError(ex, "Error in GET /api/simulation/{Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch simulation",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/scoring-criteria
        // ------------------------------------------------------------------
        [HttpGet("scoring-criteria")]
        public IActionResult GetScoringCriteria()
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
                _logger.LogError(ex, "Error in GET /api/scoring-criteria");
                return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
                {
                    Error = "Failed to fetch scoring criteria",
                    Message = ex.Message
                });
            }
        }

        // ------------------------------------------------------------------
        // GET /api/health
        // ------------------------------------------------------------------
        [HttpGet("health")]
        public IActionResult Health()
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
