using System.Threading.Tasks;
using CreditSim.Core.Models;
using CreditSim.Core.Services;
using CreditSim.Data.Repositories;

namespace CreditSim.Data
{
    /// <summary>
    /// Seeds the file-based customer store with initial data if it is empty.
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly string _filePath;
        private readonly ICreditScoringService _scoringService;

        public DatabaseInitializer(string filePath, ICreditScoringService scoringService)
        {
            _filePath = filePath;
            _scoringService = scoringService;
        }

        public async Task InitializeAsync()
        {
            await SeedIfNeededAsync();
        }

        private async Task SeedIfNeededAsync()
        {
            var repo = new CustomerRepository(_filePath);
            int count = await repo.CountAsync();

            if (count >= 30)
            {
                System.Diagnostics.Trace.TraceInformation(
                    $"Customer store already has {count} simulations, skipping seed");
                return;
            }

            System.Diagnostics.Trace.TraceInformation(
                $"Customer store has {count} simulations, seeding {SeedData.Length} records...");

            foreach (var sim in SeedData)
            {
                var result = _scoringService.CalculateCreditScore(sim);
                await repo.InsertAsync(new Customer
                {
                    Name = sim.Name,
                    Age = sim.Age,
                    AnnualIncome = sim.AnnualIncome,
                    DebtToIncomeRatio = sim.DebtToIncomeRatio,
                    LoanAmount = sim.LoanAmount,
                    CreditHistory = sim.CreditHistory,
                    Score = result.Score,
                    RiskCategory = result.RiskCategory
                });
            }

            System.Diagnostics.Trace.TraceInformation(
                $"Seeded {SeedData.Length} simulations successfully");
        }

        /// <summary>Seed records — mirrors SEED_SIMULATIONS in src/database/seed.js.</summary>
        private static readonly SimulationRequest[] SeedData = new[]
        {
            new SimulationRequest { Name = "Alice Johnson",     Age = 34, AnnualIncome = 85000,  DebtToIncomeRatio = 0.15, LoanAmount = 12000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Bob Martinez",      Age = 22, AnnualIncome = 32000,  DebtToIncomeRatio = 0.45, LoanAmount = 18000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Carol Williams",    Age = 45, AnnualIncome = 120000, DebtToIncomeRatio = 0.10, LoanAmount = 20000,  CreditHistory = "good" },
            new SimulationRequest { Name = "David Brown",       Age = 58, AnnualIncome = 65000,  DebtToIncomeRatio = 0.35, LoanAmount = 30000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Eva Davis",         Age = 29, AnnualIncome = 48000,  DebtToIncomeRatio = 0.50, LoanAmount = 25000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Frank Wilson",      Age = 41, AnnualIncome = 95000,  DebtToIncomeRatio = 0.20, LoanAmount = 15000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Grace Moore",       Age = 63, AnnualIncome = 55000,  DebtToIncomeRatio = 0.30, LoanAmount = 10000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Henry Taylor",      Age = 37, AnnualIncome = 150000, DebtToIncomeRatio = 0.08, LoanAmount = 8000,   CreditHistory = "good" },
            new SimulationRequest { Name = "Isabella Anderson", Age = 25, AnnualIncome = 40000,  DebtToIncomeRatio = 0.42, LoanAmount = 22000,  CreditHistory = "good" },
            new SimulationRequest { Name = "James Thomas",      Age = 52, AnnualIncome = 72000,  DebtToIncomeRatio = 0.25, LoanAmount = 18000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Karen Jackson",     Age = 31, AnnualIncome = 38000,  DebtToIncomeRatio = 0.55, LoanAmount = 28000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Liam White",        Age = 44, AnnualIncome = 210000, DebtToIncomeRatio = 0.05, LoanAmount = 5000,   CreditHistory = "good" },
            new SimulationRequest { Name = "Mia Harris",        Age = 27, AnnualIncome = 58000,  DebtToIncomeRatio = 0.18, LoanAmount = 12000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Noah Martin",       Age = 60, AnnualIncome = 44000,  DebtToIncomeRatio = 0.48, LoanAmount = 35000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Olivia Garcia",     Age = 36, AnnualIncome = 105000, DebtToIncomeRatio = 0.12, LoanAmount = 9000,   CreditHistory = "good" },
            new SimulationRequest { Name = "Peter Martinez",    Age = 23, AnnualIncome = 29000,  DebtToIncomeRatio = 0.60, LoanAmount = 20000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Quinn Robinson",    Age = 48, AnnualIncome = 88000,  DebtToIncomeRatio = 0.22, LoanAmount = 14000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Rachel Clark",      Age = 33, AnnualIncome = 62000,  DebtToIncomeRatio = 0.38, LoanAmount = 19000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Samuel Lewis",      Age = 56, AnnualIncome = 130000, DebtToIncomeRatio = 0.09, LoanAmount = 7500,   CreditHistory = "good" },
            new SimulationRequest { Name = "Tina Lee",          Age = 26, AnnualIncome = 36000,  DebtToIncomeRatio = 0.52, LoanAmount = 24000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Ulysses Walker",    Age = 40, AnnualIncome = 77000,  DebtToIncomeRatio = 0.28, LoanAmount = 16000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Victoria Hall",     Age = 65, AnnualIncome = 49000,  DebtToIncomeRatio = 0.33, LoanAmount = 11000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "William Allen",     Age = 39, AnnualIncome = 175000, DebtToIncomeRatio = 0.07, LoanAmount = 6000,   CreditHistory = "good" },
            new SimulationRequest { Name = "Xena Young",        Age = 28, AnnualIncome = 53000,  DebtToIncomeRatio = 0.40, LoanAmount = 21000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Yusuf Hernandez",   Age = 50, AnnualIncome = 92000,  DebtToIncomeRatio = 0.17, LoanAmount = 13000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Zoe King",          Age = 21, AnnualIncome = 25000,  DebtToIncomeRatio = 0.58, LoanAmount = 30000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Aaron Wright",      Age = 43, AnnualIncome = 68000,  DebtToIncomeRatio = 0.31, LoanAmount = 17000,  CreditHistory = "good" },
            new SimulationRequest { Name = "Bella Scott",       Age = 35, AnnualIncome = 115000, DebtToIncomeRatio = 0.11, LoanAmount = 10500,  CreditHistory = "good" },
            new SimulationRequest { Name = "Carlos Green",      Age = 55, AnnualIncome = 41000,  DebtToIncomeRatio = 0.46, LoanAmount = 26000,  CreditHistory = "bad"  },
            new SimulationRequest { Name = "Diana Adams",       Age = 30, AnnualIncome = 79000,  DebtToIncomeRatio = 0.23, LoanAmount = 15500,  CreditHistory = "good" },
        };
    }
}
