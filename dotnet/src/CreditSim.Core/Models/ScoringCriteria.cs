namespace CreditSim.Core.Models
{
    /// <summary>Mirrors the object returned by getScoringCriteria() in creditScoring.js.</summary>
    public class ScoringCriteria
    {
        public int BaseScore { get; set; }
        public ScoringAdjustments Adjustments { get; set; } = new ScoringAdjustments();
        public RiskCategories RiskCategories { get; set; } = new RiskCategories();
    }

    public class ScoringAdjustments
    {
        public AgeAdjustments Age { get; set; } = new AgeAdjustments();
        public IncomeAdjustments Income { get; set; } = new IncomeAdjustments();
        public DebtAdjustments DebtToIncomeRatio { get; set; } = new DebtAdjustments();
        public CreditHistoryAdjustments CreditHistory { get; set; } = new CreditHistoryAdjustments();
        public LoanAdjustments LoanToIncomeRatio { get; set; } = new LoanAdjustments();
    }

    public class AgeAdjustments
    {
        public int Under25 { get; set; }
        public int Over60 { get; set; }
    }

    public class IncomeAdjustments
    {
        public int Over50k { get; set; }
    }

    public class DebtAdjustments
    {
        public int Over40Percent { get; set; }
    }

    public class CreditHistoryAdjustments
    {
        public int Bad { get; set; }
    }

    public class LoanAdjustments
    {
        public int Over50Percent { get; set; }
    }

    public class RiskCategories
    {
        public string LowRisk { get; set; } = string.Empty;
        public string MediumRisk { get; set; } = string.Empty;
        public string HighRisk { get; set; } = string.Empty;
    }
}
