using CreditSim.Core.Models;

namespace CreditSim.Core.Services
{
    public interface ICreditScoringService
    {
        /// <summary>Calculates credit score and risk category for the given customer data.</summary>
        SimulationResult CalculateCreditScore(SimulationRequest request);

        /// <summary>Determines risk category string from a numeric score.</summary>
        string DetermineRiskCategory(int score);

        /// <summary>Returns the scoring criteria explanation object.</summary>
        ScoringCriteria GetScoringCriteria();
    }
}
