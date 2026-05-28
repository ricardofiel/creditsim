using System.ComponentModel.DataAnnotations;

namespace CreditSim.Core.Models
{
    /// <summary>
    /// Mirrors the express-validator rules in src/routes/simulation.js.
    /// </summary>
    public class SimulationRequest
    {
        [Required(ErrorMessage = "Name must be between 1 and 100 characters")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(18, 120, ErrorMessage = "Age must be an integer between 18 and 120")]
        public int Age { get; set; }

        [Range(0.0, double.MaxValue, ErrorMessage = "Annual income must be a positive number")]
        public double AnnualIncome { get; set; }

        [Range(0.0, 1.0, ErrorMessage = "Debt-to-income ratio must be between 0 and 1")]
        public double DebtToIncomeRatio { get; set; }

        [Range(1.0, double.MaxValue, ErrorMessage = "Loan amount must be a positive number")]
        public double LoanAmount { get; set; }

        [Required(ErrorMessage = "Credit history must be either \"good\" or \"bad\"")]
        [CreditHistoryValidation]
        public string CreditHistory { get; set; } = string.Empty;
    }
}
