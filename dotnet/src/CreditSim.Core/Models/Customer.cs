using System;
using Newtonsoft.Json;

namespace CreditSim.Core.Models
{
    public class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }

        [JsonProperty("annualIncome")]
        public double AnnualIncome { get; set; }

        [JsonProperty("debtToIncomeRatio")]
        public double DebtToIncomeRatio { get; set; }

        [JsonProperty("loanAmount")]
        public double LoanAmount { get; set; }

        [JsonProperty("creditHistory")]
        public string CreditHistory { get; set; } = string.Empty;

        public int Score { get; set; }

        [JsonProperty("riskCategory")]
        public string RiskCategory { get; set; } = string.Empty;

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}
