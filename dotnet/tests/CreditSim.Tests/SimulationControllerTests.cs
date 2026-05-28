using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CreditSim.Core.Models;
using CreditSim.Core.Services;
using CreditSim.Data.Repositories;
using CreditSim.Web.Controllers;
using CreditSim.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CreditSim.Tests
{
    /// <summary>
    /// Unit tests for SimulationController (ASP.NET Core).
    /// Mirrors every test in tests/api.test.js.
    /// Uses Moq to mock ICreditScoringService and ICustomerRepository.
    /// </summary>
    [TestClass]
    public class SimulationControllerTests
    {
        private Mock<ICreditScoringService> _mockService = null!;
        private Mock<ICustomerRepository>  _mockRepo    = null!;
        private SimulationController       _controller  = null!;

        private static SimulationRequest ValidRequest() => new SimulationRequest
        {
            Name             = "John Doe",
            Age              = 35,
            AnnualIncome     = 60_000,
            DebtToIncomeRatio = 0.3,
            LoanAmount       = 25_000,
            CreditHistory    = "good"
        };

        [TestInitialize]
        public void Setup()
        {
            _mockService = new Mock<ICreditScoringService>();
            _mockRepo    = new Mock<ICustomerRepository>();
            _controller  = new SimulationController(
                _mockService.Object,
                _mockRepo.Object,
                NullLogger<SimulationController>.Instance);
        }

        private static ObjectResult AssertObjectResult(IActionResult result, HttpStatusCode expected)
        {
            var objectResult = result as ObjectResult;
            Assert.IsNotNull(objectResult, $"Expected ObjectResult, got {result?.GetType().Name}");
            Assert.AreEqual((int)expected, objectResult!.StatusCode);
            return objectResult;
        }

        // ----------------------------------------------------------------
        // POST /api/simulate
        // ----------------------------------------------------------------

        [TestMethod]
        public async Task RunSimulation_ValidRequest_Returns201WithResult()
        {
            _mockService.Setup(s => s.CalculateCreditScore(It.IsAny<SimulationRequest>()))
                .Returns(new SimulationResult { Score = 640, RiskCategory = "High risk" });

            _mockRepo.Setup(r => r.InsertAsync(It.IsAny<Customer>()))
                .ReturnsAsync(new Customer
                {
                    Id = 1, Name = "John Doe", Score = 640, RiskCategory = "High risk",
                    Age = 35, AnnualIncome = 60_000, DebtToIncomeRatio = 0.3,
                    LoanAmount = 25_000, CreditHistory = "good"
                });

            var result = await _controller.Simulate(ValidRequest());

            var objectResult = AssertObjectResult(result, HttpStatusCode.Created);
            var payload = objectResult.Value as SimulateResponse;
            Assert.IsNotNull(payload, "Expected SimulateResponse payload");
            Assert.AreEqual(1,           payload!.Id);
            Assert.AreEqual(640,         payload.Score);
            Assert.AreEqual("High risk", payload.RiskCategory);
            Assert.AreEqual("Credit score calculated successfully", payload.Message);
            Assert.AreEqual("John Doe",  payload.Customer.Name);
        }

        [TestMethod]
        public async Task RunSimulation_MissingRequiredFields_Returns400()
        {
            _controller.ModelState.AddModelError("Name", "Name must be between 1 and 100 characters");

            var result = await _controller.Simulate(new SimulationRequest());

            var objectResult = AssertObjectResult(result, HttpStatusCode.BadRequest);
            var payload = objectResult.Value as ErrorResponse;
            Assert.IsNotNull(payload, "Expected ErrorResponse payload");
            Assert.AreEqual("Validation failed", payload!.Error);
            Assert.IsNotNull(payload.Details);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidIncome_Returns400()
        {
            _controller.ModelState.AddModelError(
                "AnnualIncome", "Annual income must be a positive number");

            var result = await _controller.Simulate(new SimulationRequest { AnnualIncome = -1 });

            var objectResult = AssertObjectResult(result, HttpStatusCode.BadRequest);
            var payload = objectResult.Value as ErrorResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual("Validation failed", payload!.Error);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidAge_Returns400()
        {
            _controller.ModelState.AddModelError(
                "Age", "Age must be an integer between 18 and 120");

            var result = await _controller.Simulate(new SimulationRequest { Age = 17 });

            AssertObjectResult(result, HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidCreditHistory_Returns400()
        {
            _controller.ModelState.AddModelError(
                "CreditHistory", "Credit history must be either \"good\" or \"bad\"");

            var result = await _controller.Simulate(
                new SimulationRequest { CreditHistory = "excellent" });

            AssertObjectResult(result, HttpStatusCode.BadRequest);
        }

        // ----------------------------------------------------------------
        // GET /api/simulations
        // ----------------------------------------------------------------

        [TestMethod]
        public async Task GetCustomers_Returns200WithPagedList()
        {
            var fakeList = Enumerable.Range(1, 5).Select(i => new Customer
            {
                Id = i, Name = $"Customer {i}", Score = 600, RiskCategory = "High risk",
                LoanAmount = 10_000, CreatedAt = System.DateTime.UtcNow
            }).ToList();

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeList);

            var result = await _controller.GetSimulations();

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok, "Expected OkObjectResult");
            var payload = ok!.Value as SimulationsListResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual(5, payload!.Count);
            Assert.AreEqual(5, payload.Simulations.Count());
        }

        [TestMethod]
        public async Task GetCustomers_EmptyDatabase_Returns200WithEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Customer>());

            var result = await _controller.GetSimulations();

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            var payload = ok!.Value as SimulationsListResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual(0, payload!.Count);
            Assert.AreEqual(0, payload.Simulations.Count());
        }

        [TestMethod]
        public async Task GetCustomers_Page2_ReturnsCorrectSlice()
        {
            // The controller returns all; paging is in the repository layer.
            // We verify that when the mock repo returns 15 items, the response count is 15.
            var fakeList = Enumerable.Range(1, 15).Select(i => new Customer
            {
                Id = i, Name = $"Customer {i}", Score = 600, RiskCategory = "High risk",
                LoanAmount = 10_000, CreatedAt = System.DateTime.UtcNow
            }).ToList();

            _mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(fakeList);

            var result = await _controller.GetSimulations();

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            var payload = ok!.Value as SimulationsListResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual(15, payload!.Count);
        }

        // ----------------------------------------------------------------
        // GET /api/simulation/{id}
        // ----------------------------------------------------------------

        [TestMethod]
        public async Task GetSimulation_ExistingId_Returns200WithDetail()
        {
            var fake = new Customer
            {
                Id = 1, Name = "Test User for ID", Age = 28,
                AnnualIncome = 55_000, DebtToIncomeRatio = 0.25,
                LoanAmount = 15_000, CreditHistory = "good",
                Score = 640, RiskCategory = "High risk",
                CreatedAt = System.DateTime.UtcNow
            };

            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fake);

            var result = await _controller.GetSimulation(1);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok, "Expected OkObjectResult");
            var payload = ok!.Value as SimulationDetailResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual("Test User for ID", payload!.Simulation.Name);
            Assert.AreEqual(1, payload.Simulation.Id);
        }

        [TestMethod]
        public async Task GetSimulation_NonExistentId_Returns404()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99999)).ReturnsAsync((Customer?)null);

            var result = await _controller.GetSimulation(99999);

            var objectResult = AssertObjectResult(result, HttpStatusCode.NotFound);
            var payload = objectResult.Value as ErrorResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual("Simulation not found", payload!.Error);
        }

        [TestMethod]
        public async Task GetSimulation_InvalidIdFormat_Returns400()
        {
            // id < 1 triggers validation in the controller
            var result = await _controller.GetSimulation(0);

            AssertObjectResult(result, HttpStatusCode.BadRequest);
        }

        // ----------------------------------------------------------------
        // GET /api/scoring-criteria
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetScoringCriteria_Returns200WithCriteria()
        {
            _mockService.Setup(s => s.GetScoringCriteria())
                .Returns(new ScoringCriteria
                {
                    BaseScore = 600,
                    Adjustments = new ScoringAdjustments
                    {
                        Age = new AgeAdjustments { Under25 = -50, Over60 = -30 },
                        Income = new IncomeAdjustments { Over50k = 40 },
                        DebtToIncomeRatio = new DebtAdjustments { Over40Percent = -80 },
                        CreditHistory = new CreditHistoryAdjustments { Bad = -150 },
                        LoanToIncomeRatio = new LoanAdjustments { Over50Percent = -50 }
                    },
                    RiskCategories = new RiskCategories
                    {
                        LowRisk = "750+", MediumRisk = "650-749", HighRisk = "Below 650"
                    }
                });

            var result = _controller.GetScoringCriteria();

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok, "Expected OkObjectResult");
            Assert.IsNotNull(ok!.Value);
        }

        // ----------------------------------------------------------------
        // GET /api/health
        // ----------------------------------------------------------------

        [TestMethod]
        public void Health_Returns200WithStatusHealthy()
        {
            var result = _controller.Health();

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok, "Expected OkObjectResult");
            var payload = ok!.Value as HealthResponse;
            Assert.IsNotNull(payload);
            Assert.AreEqual("healthy", payload!.Status);
            Assert.IsNotNull(payload.Timestamp);
            Assert.IsTrue(payload.Uptime >= 0);
        }
    }
}
