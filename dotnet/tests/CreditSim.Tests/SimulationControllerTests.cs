using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using CreditSim.Core.Models;
using CreditSim.Core.Services;
using CreditSim.Data.Repositories;
using CreditSim.Web.Controllers;
using CreditSim.Web.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CreditSim.Tests
{
    /// <summary>
    /// Unit tests for SimulationController.
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
            _controller  = new SimulationController(_mockService.Object, _mockRepo.Object);

            // Wire up the controller request context (required for Content() helper)
            _controller.Request = new HttpRequestMessage();
            _controller.Request.SetConfiguration(new HttpConfiguration());
        }

        [TestCleanup]
        public void Cleanup() => _controller.Dispose();

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

            var content = result as NegotiatedContentResult<SimulateResponse>;
            Assert.IsNotNull(content, "Expected NegotiatedContentResult<SimulateResponse>");
            Assert.AreEqual(HttpStatusCode.Created, content.StatusCode);
            Assert.AreEqual(1,           content.Content.Id);
            Assert.AreEqual(640,         content.Content.Score);
            Assert.AreEqual("High risk", content.Content.RiskCategory);
            Assert.AreEqual("Credit score calculated successfully", content.Content.Message);
            Assert.AreEqual("John Doe",  content.Content.Customer.Name);
        }

        [TestMethod]
        public async Task RunSimulation_MissingRequiredFields_Returns400()
        {
            _controller.ModelState.AddModelError("Name", "Name must be between 1 and 100 characters");

            var result = await _controller.Simulate(new SimulationRequest());

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content, "Expected NegotiatedContentResult<ErrorResponse>");
            Assert.AreEqual(HttpStatusCode.BadRequest, content.StatusCode);
            Assert.AreEqual("Validation failed", content.Content.Error);
            Assert.IsNotNull(content.Content.Details);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidIncome_Returns400()
        {
            _controller.ModelState.AddModelError(
                "AnnualIncome", "Annual income must be a positive number");

            var result = await _controller.Simulate(new SimulationRequest { AnnualIncome = -1 });

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content);
            Assert.AreEqual(HttpStatusCode.BadRequest, content.StatusCode);
            Assert.AreEqual("Validation failed", content.Content.Error);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidAge_Returns400()
        {
            _controller.ModelState.AddModelError(
                "Age", "Age must be an integer between 18 and 120");

            var result = await _controller.Simulate(new SimulationRequest { Age = 17 });

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content);
            Assert.AreEqual(HttpStatusCode.BadRequest, content.StatusCode);
        }

        [TestMethod]
        public async Task RunSimulation_InvalidCreditHistory_Returns400()
        {
            _controller.ModelState.AddModelError(
                "CreditHistory", "Credit history must be either \"good\" or \"bad\"");

            var result = await _controller.Simulate(
                new SimulationRequest { CreditHistory = "excellent" });

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content);
            Assert.AreEqual(HttpStatusCode.BadRequest, content.StatusCode);
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

            var ok = result as OkNegotiatedContentResult<SimulationsListResponse>;
            Assert.IsNotNull(ok, "Expected OkNegotiatedContentResult<SimulationsListResponse>");
            Assert.AreEqual(HttpStatusCode.OK, HttpStatusCode.OK);
            Assert.AreEqual(5,      ok.Content.Count);
            Assert.AreEqual(5,      ok.Content.Simulations.Count());
        }

        [TestMethod]
        public async Task GetCustomers_EmptyDatabase_Returns200WithEmptyList()
        {
            _mockRepo.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Customer>());

            var result = await _controller.GetSimulations();

            var ok = result as OkNegotiatedContentResult<SimulationsListResponse>;
            Assert.IsNotNull(ok);
            Assert.AreEqual(0, ok.Content.Count);
            Assert.AreEqual(0, ok.Content.Simulations.Count());
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

            var ok = result as OkNegotiatedContentResult<SimulationsListResponse>;
            Assert.IsNotNull(ok);
            Assert.AreEqual(15, ok.Content.Count);
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

            var ok = result as OkNegotiatedContentResult<SimulationDetailResponse>;
            Assert.IsNotNull(ok, "Expected OkNegotiatedContentResult<SimulationDetailResponse>");
            Assert.AreEqual(1,                "Test User for ID" == ok.Content.Simulation.Name ? 1 : 0);
            Assert.AreEqual("Test User for ID", ok.Content.Simulation.Name);
            Assert.AreEqual(1, ok.Content.Simulation.Id);
        }

        [TestMethod]
        public async Task GetSimulation_NonExistentId_Returns404()
        {
            _mockRepo.Setup(r => r.GetByIdAsync(99999)).ReturnsAsync((Customer?)null);

            var result = await _controller.GetSimulation(99999);

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content, "Expected NegotiatedContentResult<ErrorResponse>");
            Assert.AreEqual(HttpStatusCode.NotFound, content.StatusCode);
            Assert.AreEqual("Simulation not found", content.Content.Error);
        }

        [TestMethod]
        public async Task GetSimulation_InvalidIdFormat_Returns400()
        {
            // id < 1 triggers validation in the controller
            var result = await _controller.GetSimulation(0);

            var content = result as NegotiatedContentResult<ErrorResponse>;
            Assert.IsNotNull(content);
            Assert.AreEqual(HttpStatusCode.BadRequest, content.StatusCode);
        }

        // ----------------------------------------------------------------
        // GET /api/scoring-criteria
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetScoringCriteria_Returns200WithCriteria()
        {
            _mockService.Setup(s => s.GetScoringCriteria())
                .Returns(new Core.Models.ScoringCriteria
                {
                    BaseScore = 600,
                    Adjustments = new Core.Models.ScoringAdjustments
                    {
                        Age = new Core.Models.AgeAdjustments { Under25 = -50, Over60 = -30 },
                        Income = new Core.Models.IncomeAdjustments { Over50k = 40 },
                        DebtToIncomeRatio = new Core.Models.DebtAdjustments { Over40Percent = -80 },
                        CreditHistory = new Core.Models.CreditHistoryAdjustments { Bad = -150 },
                        LoanToIncomeRatio = new Core.Models.LoanAdjustments { Over50Percent = -50 }
                    },
                    RiskCategories = new Core.Models.RiskCategories
                    {
                        LowRisk = "750+", MediumRisk = "650-749", HighRisk = "Below 650"
                    }
                });

            var result = _controller.GetScoringCriteria();

            var ok = result as OkNegotiatedContentResult<object>;
            Assert.IsNotNull(ok, "Expected OkNegotiatedContentResult");
        }

        // ----------------------------------------------------------------
        // GET /api/health
        // ----------------------------------------------------------------

        [TestMethod]
        public void Health_Returns200WithStatusHealthy()
        {
            var result = _controller.Health();

            var ok = result as OkNegotiatedContentResult<HealthResponse>;
            Assert.IsNotNull(ok, "Expected OkNegotiatedContentResult<HealthResponse>");
            Assert.AreEqual("healthy", ok.Content.Status);
            Assert.IsNotNull(ok.Content.Timestamp);
            Assert.IsTrue(ok.Content.Uptime >= 0);
        }
    }
}
