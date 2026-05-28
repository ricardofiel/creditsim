using System.Collections.Generic;

namespace CreditSim.Web.Models
{
    // ---------- POST /api/simulate ----------

    public class SimulateResponse
    {
        public int Id { get; set; }
        public int Score { get; set; }
        public string RiskCategory { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public SimulateCustomerDto Customer { get; set; } = new SimulateCustomerDto();
    }

    public class SimulateCustomerDto
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double AnnualIncome { get; set; }
        public double DebtToIncomeRatio { get; set; }
        public double LoanAmount { get; set; }
        public string CreditHistory { get; set; } = string.Empty;
    }

    // ---------- GET /api/simulations ----------

    public class SimulationsListResponse
    {
        public int Count { get; set; }
        public IEnumerable<SimulationSummaryDto> Simulations { get; set; }
            = new List<SimulationSummaryDto>();
    }

    public class SimulationSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Score { get; set; }
        public string RiskCategory { get; set; } = string.Empty;
        public double LoanAmount { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
    }

    // ---------- GET /api/simulation/:id ----------

    public class SimulationDetailResponse
    {
        public SimulationDetailDto Simulation { get; set; } = new SimulationDetailDto();
    }

    public class SimulationDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double AnnualIncome { get; set; }
        public double DebtToIncomeRatio { get; set; }
        public double LoanAmount { get; set; }
        public string CreditHistory { get; set; } = string.Empty;
        public int Score { get; set; }
        public string RiskCategory { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    // ---------- GET /api/health ----------

    public class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
        public double Uptime { get; set; }
    }

    // ---------- Error responses ----------

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
        public object? Details { get; set; }
        public string? Message { get; set; }
    }
}

