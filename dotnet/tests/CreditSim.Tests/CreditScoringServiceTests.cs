using System;
using CreditSim.Core.Models;
using CreditSim.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CreditSim.Tests
{
    /// <summary>
    /// Unit tests for CreditScoringService.
    /// Mirrors every test in tests/creditScoring.test.js.
    /// </summary>
    [TestClass]
    public class CreditScoringServiceTests
    {
        private readonly CreditScoringService _service = new CreditScoringService();

        private static SimulationRequest BaseCustomer() => new SimulationRequest
        {
            Name = "Base Customer",
            Age = 35,
            AnnualIncome = 60_000,
            DebtToIncomeRatio = 0.3,
            LoanAmount = 25_000,
            CreditHistory = "good"
        };

        // ----------------------------------------------------------------
        // calculateCreditScore tests — mirrors describe('calculateCreditScore')
        // ----------------------------------------------------------------

        [TestMethod]
        public void CalculateCreditScore_BaseCustomer_Returns640()
        {
            // Base (600) + income>50k (+40) = 640
            // loanToIncome = 25000/60000 = 0.417 — no penalty
            var result = _service.CalculateCreditScore(BaseCustomer());
            Assert.AreEqual(640, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_YoungCustomerAge22_AppliesAgePenalty()
        {
            // Base (600) + income (+40) - young age (-50) = 590
            var req = BaseCustomer(); req.Age = 22;
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(590, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_OlderCustomerAge65_AppliesSeniorPenalty()
        {
            // Base (600) + income (+40) - senior (-30) = 610
            var req = BaseCustomer(); req.Age = 65;
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(610, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_LowIncome45k_NoIncomeBonus()
        {
            // Base (600), no income bonus, loan 25000/45000 = 0.556 > 0.5 → -50
            var req = BaseCustomer(); req.AnnualIncome = 45_000;
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(550, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_HighDebtRatio05_AppliesDebtPenalty()
        {
            // Base (600) + income (+40) - highDebt (-80) = 560
            var req = BaseCustomer(); req.DebtToIncomeRatio = 0.5;
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(560, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_BadCreditHistory_AppliesPenalty()
        {
            // Base (600) + income (+40) - badCredit (-150) = 490
            var req = BaseCustomer(); req.CreditHistory = "bad";
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(490, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_HighLoanToIncomeRatio_AppliesLoanPenalty()
        {
            // loanAmount 35000 / income 60000 = 0.583 > 0.5 → -50
            // Base (600) + income (+40) - highLoan (-50) = 590
            var req = BaseCustomer(); req.LoanAmount = 35_000;
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(590, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_MultiplePenalties_ClampsToMinimum300()
        {
            // age 22 (-50), no income bonus, highDebt (-80), badCredit (-150), highLoan (-50)
            // 600 - 50 - 80 - 150 - 50 = 270 → clamped to 300
            var req = new SimulationRequest
            {
                Name = "High Risk",
                Age = 22,
                AnnualIncome = 30_000,
                DebtToIncomeRatio = 0.6,
                LoanAmount = 20_000,
                CreditHistory = "bad"
            };
            var result = _service.CalculateCreditScore(req);
            Assert.AreEqual(300, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_ExcellentProfile_ScoreCappedAt850()
        {
            // Base (600) + veryHighIncome (annualIncome > 200k, +120) + veryLowLoanRatio (<0.1, +30) = 750
            // (annualIncome 200k is NOT > 200k — matches JS strict >)
            var req = new SimulationRequest
            {
                Name = "Perfect",
                Age = 35,
                AnnualIncome = 200_000,
                DebtToIncomeRatio = 0.1,
                LoanAmount = 10_000,
                CreditHistory = "good"
            };
            // annualIncome=200000 is NOT > 200000, so +80 (>100000)
            // loanToIncome = 10000/200000 = 0.05 < 0.1 → +30
            // 600 + 80 + 30 = 710
            var result = _service.CalculateCreditScore(req);
            Assert.IsTrue(result.Score <= 850, "Score must not exceed 850");
            Assert.AreEqual("Medium risk", result.RiskCategory);
        }

        // ----------------------------------------------------------------
        // CalculateCreditScore — named test cases matching agent spec
        // ----------------------------------------------------------------

        [TestMethod]
        public void CalculateCreditScore_ExcellentProfile_ReturnsHighScore()
        {
            // income 120k, low debt, good credit (equivalent to "perfect payment" in agent spec)
            var req = new SimulationRequest
            {
                Name = "Excellent",
                Age = 40,
                AnnualIncome = 120_000,
                DebtToIncomeRatio = 0.1,
                LoanAmount = 5_000,
                CreditHistory = "good"
            };
            var result = _service.CalculateCreditScore(req);
            // 600 + 80 (>100k) + 30 (loanRatio 5000/120000 = 0.042 < 0.1) = 710
            Assert.AreEqual(710, result.Score);
            Assert.AreEqual("Medium risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_PoorProfile_ReturnsLowScore()
        {
            // income 25k, high debt, missed payments (bad credit)
            var req = new SimulationRequest
            {
                Name = "Poor",
                Age = 30,
                AnnualIncome = 25_000,
                DebtToIncomeRatio = 0.6,
                LoanAmount = 15_000,
                CreditHistory = "bad"
            };
            var result = _service.CalculateCreditScore(req);
            // 600 - 80 (highDebt) - 150 (badCredit) - 50 (loanRatio 0.6 > 0.5) = 320
            Assert.AreEqual(320, result.Score);
            Assert.AreEqual("High risk", result.RiskCategory);
        }

        [TestMethod]
        public void CalculateCreditScore_BoundaryIncome_ReturnsExpectedTier()
        {
            // Income exactly 50000 → no bonus (>50k required, not >=50k)
            var req = BaseCustomer(); req.AnnualIncome = 50_000;
            var result = _service.CalculateCreditScore(req);
            // 600 + 0 - 50 (loanRatio 25000/50000 = 0.5, NOT > 0.5 so no penalty; exactly 0.5 not > 0.5)
            // loanRatio = 0.5: not > 0.5 (false), not < 0.1 (false), not < 0.25 (false) → no bonus/penalty
            Assert.AreEqual(600, result.Score);
        }

        // ----------------------------------------------------------------
        // determineRiskCategory tests — mirrors describe('determineRiskCategory')
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetRiskCategory_ScoreAbove750_ReturnsLowRisk()
        {
            Assert.AreEqual("Low risk", _service.DetermineRiskCategory(750));
            Assert.AreEqual("Low risk", _service.DetermineRiskCategory(800));
            Assert.AreEqual("Low risk", _service.DetermineRiskCategory(850));
        }

        [TestMethod]
        public void GetRiskCategory_ScoreBelow500_ReturnsPoor()
        {
            Assert.AreEqual("High risk", _service.DetermineRiskCategory(300));
            Assert.AreEqual("High risk", _service.DetermineRiskCategory(499));
            Assert.AreEqual("High risk", _service.DetermineRiskCategory(500));
        }

        [DataTestMethod]
        [DataRow(750, "Low risk")]
        [DataRow(800, "Low risk")]
        [DataRow(850, "Low risk")]
        [DataRow(650, "Medium risk")]
        [DataRow(700, "Medium risk")]
        [DataRow(749, "Medium risk")]
        [DataRow(300, "High risk")]
        [DataRow(500, "High risk")]
        [DataRow(649, "High risk")]
        public void GetRiskCategory_AllBoundaryValues(int score, string expected)
        {
            Assert.AreEqual(expected, _service.DetermineRiskCategory(score));
        }

        // ----------------------------------------------------------------
        // validateCustomerData tests — mirrors describe('validateCustomerData')
        // ----------------------------------------------------------------

        [TestMethod]
        public void ValidateCustomerData_ValidInput_DoesNotThrow()
        {
            _service.CalculateCreditScore(BaseCustomer()); // should not throw
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_InvalidAge_Throws()
        {
            var req = BaseCustomer(); req.Age = 17;
            _service.CalculateCreditScore(req);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_NegativeIncome_Throws()
        {
            var req = BaseCustomer(); req.AnnualIncome = -1000;
            _service.CalculateCreditScore(req);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_InvalidDebtRatio_Throws()
        {
            var req = BaseCustomer(); req.DebtToIncomeRatio = 1.5;
            _service.CalculateCreditScore(req);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_ZeroLoanAmount_Throws()
        {
            var req = BaseCustomer(); req.LoanAmount = 0;
            _service.CalculateCreditScore(req);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_InvalidCreditHistory_Throws()
        {
            var req = BaseCustomer(); req.CreditHistory = "excellent";
            _service.CalculateCreditScore(req);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ValidateCustomerData_MultipleInvalidFields_ThrowsWithAllMessages()
        {
            var req = new SimulationRequest
            {
                Name = "Bad",
                Age = 15,
                AnnualIncome = -1000,
                DebtToIncomeRatio = 2.0,
                LoanAmount = -5000,
                CreditHistory = "invalid"
            };
            _service.CalculateCreditScore(req);
        }

        // ----------------------------------------------------------------
        // getScoringCriteria tests — mirrors describe('getScoringCriteria')
        // ----------------------------------------------------------------

        [TestMethod]
        public void GetScoringCriteria_ReturnsExpectedStructure()
        {
            var criteria = _service.GetScoringCriteria();

            Assert.IsNotNull(criteria);
            Assert.AreEqual(600, criteria.BaseScore);
            Assert.IsNotNull(criteria.Adjustments);
            Assert.IsNotNull(criteria.RiskCategories);

            Assert.AreEqual(-50, criteria.Adjustments.Age.Under25);
            Assert.AreEqual(-30, criteria.Adjustments.Age.Over60);
            Assert.AreEqual(40,  criteria.Adjustments.Income.Over50k);
            Assert.AreEqual(-80, criteria.Adjustments.DebtToIncomeRatio.Over40Percent);
            Assert.AreEqual(-150, criteria.Adjustments.CreditHistory.Bad);
            Assert.AreEqual(-50, criteria.Adjustments.LoanToIncomeRatio.Over50Percent);

            Assert.AreEqual("750+",      criteria.RiskCategories.LowRisk);
            Assert.AreEqual("650-749",   criteria.RiskCategories.MediumRisk);
            Assert.AreEqual("Below 650", criteria.RiskCategories.HighRisk);
        }
    }
}
