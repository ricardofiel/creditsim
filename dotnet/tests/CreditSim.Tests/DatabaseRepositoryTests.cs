using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CreditSim.Core.Models;
using CreditSim.Data.Repositories;
using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CreditSim.Tests
{
    /// <summary>
    /// Integration tests for CustomerRepository against a real (temp-file) SQLite database.
    /// </summary>
    [TestClass]
    public class DatabaseRepositoryTests
    {
        private string _dbPath = string.Empty;
        private string _connectionString = string.Empty;
        private CustomerRepository _repo = null!;

        [TestInitialize]
        public async Task Setup()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"creditsim_test_{Guid.NewGuid():N}.db");
            _connectionString = $"Data Source={_dbPath};Version=3;";
            _repo = new CustomerRepository(_connectionString);

            // Create the schema for each test
            using var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            await conn.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS customers (
                    id               INTEGER PRIMARY KEY AUTOINCREMENT,
                    name             TEXT    NOT NULL,
                    age              INTEGER NOT NULL,
                    annualIncome     REAL    NOT NULL,
                    debtToIncomeRatio REAL   NOT NULL,
                    loanAmount       REAL    NOT NULL,
                    creditHistory    TEXT    NOT NULL,
                    score            INTEGER NOT NULL,
                    riskCategory     TEXT    NOT NULL,
                    createdAt        DATETIME DEFAULT CURRENT_TIMESTAMP
                )");
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (File.Exists(_dbPath)) File.Delete(_dbPath); }
            catch { /* best-effort cleanup */ }
        }

        // ----------------------------------------------------------------

        [TestMethod]
        public async Task SaveAndRetrieve_Customer_RoundTripsCorrectly()
        {
            var customer = new Customer
            {
                Name             = "Test User",
                Age              = 30,
                AnnualIncome     = 60_000,
                DebtToIncomeRatio = 0.3,
                LoanAmount       = 25_000,
                CreditHistory    = "good",
                Score            = 640,
                RiskCategory     = "High risk"
            };

            var saved = await _repo.InsertAsync(customer);

            Assert.IsTrue(saved.Id > 0, "Saved customer should have an auto-generated Id");

            var retrieved = await _repo.GetByIdAsync(saved.Id);

            Assert.IsNotNull(retrieved);
            Assert.AreEqual("Test User",  retrieved.Name);
            Assert.AreEqual(30,           retrieved.Age);
            Assert.AreEqual(60_000,       retrieved.AnnualIncome);
            Assert.AreEqual(0.3,          retrieved.DebtToIncomeRatio);
            Assert.AreEqual(25_000,       retrieved.LoanAmount);
            Assert.AreEqual("good",       retrieved.CreditHistory);
            Assert.AreEqual(640,          retrieved.Score);
            Assert.AreEqual("High risk",  retrieved.RiskCategory);
        }

        [TestMethod]
        public async Task GetPagedCustomers_ReturnsCorrectPage()
        {
            // Insert 15 customers
            for (int i = 1; i <= 15; i++)
            {
                await _repo.InsertAsync(new Customer
                {
                    Name = $"Customer {i}", Age = 30, AnnualIncome = 50_000,
                    DebtToIncomeRatio = 0.3, LoanAmount = 10_000,
                    CreditHistory = "good", Score = 600, RiskCategory = "High risk"
                });
            }

            // Page 0: first 10
            var page0 = (await _repo.GetPagedAsync(0, 10)).ToList();
            Assert.AreEqual(10, page0.Count, "Page 0 should return 10 records");

            // Page 1: remaining 5
            var page1 = (await _repo.GetPagedAsync(1, 10)).ToList();
            Assert.AreEqual(5, page1.Count, "Page 1 should return 5 records");
        }

        [TestMethod]
        public async Task GetPagedCustomers_EmptyTable_ReturnsEmptyList()
        {
            var page = (await _repo.GetPagedAsync(0, 10)).ToList();
            Assert.AreEqual(0, page.Count, "Empty table should return empty page");
        }

        [TestMethod]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            Assert.AreEqual(0, await _repo.CountAsync());

            await _repo.InsertAsync(new Customer
            {
                Name = "A", Age = 25, AnnualIncome = 40_000,
                DebtToIncomeRatio = 0.2, LoanAmount = 10_000,
                CreditHistory = "good", Score = 600, RiskCategory = "High risk"
            });

            Assert.AreEqual(1, await _repo.CountAsync());
        }

        [TestMethod]
        public async Task GetAllAsync_OrdersByCreatedAtDesc()
        {
            for (int i = 1; i <= 3; i++)
            {
                await _repo.InsertAsync(new Customer
                {
                    Name = $"Customer {i}", Age = 30, AnnualIncome = 50_000,
                    DebtToIncomeRatio = 0.3, LoanAmount = 10_000,
                    CreditHistory = "good", Score = 600, RiskCategory = "High risk"
                });
            }

            var all = (await _repo.GetAllAsync()).ToList();

            Assert.AreEqual(3, all.Count);
            // Most recent (highest id) should come first
            Assert.IsTrue(all[0].Id >= all[1].Id,
                "GetAllAsync should return records ordered by createdAt DESC");
        }

        [TestMethod]
        public async Task GetByIdAsync_NonExistentId_ReturnsNull()
        {
            var result = await _repo.GetByIdAsync(99999);
            Assert.IsNull(result, "Non-existent id should return null");
        }
    }
}
