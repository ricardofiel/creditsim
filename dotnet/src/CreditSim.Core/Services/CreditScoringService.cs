using System;
using CreditSim.Core.Models;

namespace CreditSim.Core.Services
{
    /// <summary>
    /// Pure business logic for credit score calculation.
    /// Ported 1:1 from src/services/creditScoring.js.
    /// </summary>
    public class CreditScoringService : ICreditScoringService
    {
        /// <inheritdoc />
        public SimulationResult CalculateCreditScore(SimulationRequest request)
        {
            ValidateCustomerData(request);

            int score = 600; // base score

            // Age adjustments
            if (request.Age < 25)
                score -= 50; // young age penalty
            else if (request.Age > 60)
                score -= 30; // senior age penalty

            // Income adjustments
            if (request.AnnualIncome > 200_000)
                score += 120; // very high income bonus
            else if (request.AnnualIncome > 100_000)
                score += 80;  // high income bonus
            else if (request.AnnualIncome > 50_000)
                score += 40;  // moderate income bonus

            // Debt-to-income ratio adjustments
            if (request.DebtToIncomeRatio > 0.4)
                score -= 80; // high debt-to-income penalty

            // Credit history adjustments
            if (request.CreditHistory == "bad")
                score -= 150; // bad credit history penalty

            // Loan-to-income ratio adjustments
            double loanToIncomeRatio = request.LoanAmount / request.AnnualIncome;
            if (loanToIncomeRatio > 0.5)
                score -= 50; // high loan-to-income penalty
            else if (loanToIncomeRatio < 0.1)
                score += 30; // very low loan-to-income bonus
            else if (loanToIncomeRatio < 0.25)
                score += 15; // low loan-to-income bonus

            // Clamp to FICO range
            score = Math.Max(300, Math.Min(850, score));

            return new SimulationResult
            {
                Score = (int)Math.Round((double)score),
                RiskCategory = DetermineRiskCategory(score)
            };
        }

        /// <inheritdoc />
        public string DetermineRiskCategory(int score)
        {
            if (score >= 750) return "Low risk";
            if (score >= 650) return "Medium risk";
            return "High risk";
        }

        /// <inheritdoc />
        public ScoringCriteria GetScoringCriteria()
        {
            return new ScoringCriteria
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
                    LowRisk = "750+",
                    MediumRisk = "650-749",
                    HighRisk = "Below 650"
                }
            };
        }

        /// <summary>
        /// Validates customer data inputs; throws <see cref="ArgumentException"/> if invalid.
        /// Mirrors the validateCustomerData() function in creditScoring.js.
        /// </summary>
        private static void ValidateCustomerData(SimulationRequest request)
        {
            var errors = new System.Collections.Generic.List<string>();

            if (request.Age < 18 || request.Age > 120)
                errors.Add("Age must be an integer between 18 and 120");

            if (double.IsNaN(request.AnnualIncome) || double.IsInfinity(request.AnnualIncome) || request.AnnualIncome < 0)
                errors.Add("Annual income must be a positive number");

            if (double.IsNaN(request.DebtToIncomeRatio) || double.IsInfinity(request.DebtToIncomeRatio)
                || request.DebtToIncomeRatio < 0
                || request.DebtToIncomeRatio > 1)
                errors.Add("Debt-to-income ratio must be between 0 and 1");

            if (double.IsNaN(request.LoanAmount) || double.IsInfinity(request.LoanAmount) || request.LoanAmount <= 0)
                errors.Add("Loan amount must be a positive number");

            if (request.CreditHistory != "good" && request.CreditHistory != "bad")
                errors.Add("Credit history must be either \"good\" or \"bad\"");

            if (errors.Count > 0)
                throw new ArgumentException($"Validation failed: {string.Join(", ", errors)}");
        }
    }
}
