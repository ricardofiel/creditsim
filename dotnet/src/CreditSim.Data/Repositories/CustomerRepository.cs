using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;
using CreditSim.Core.Models;
using Dapper;

namespace CreditSim.Data.Repositories
{
    /// <summary>
    /// SQLite implementation of <see cref="ICustomerRepository"/> using Dapper.
    /// Mirrors the Database class in src/database/database.js.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private SQLiteConnection OpenConnection()
        {
            var conn = new SQLiteConnection(_connectionString);
            conn.Open();
            conn.Execute("PRAGMA foreign_keys = ON");
            return conn;
        }

        /// <inheritdoc />
        public async Task<Customer> InsertAsync(Customer customer)
        {
            const string sql = @"
                INSERT INTO customers (name, age, annualIncome, debtToIncomeRatio, loanAmount, creditHistory, score, riskCategory)
                VALUES (@Name, @Age, @AnnualIncome, @DebtToIncomeRatio, @LoanAmount, @CreditHistory, @Score, @RiskCategory);
                SELECT last_insert_rowid();";

            using var conn = OpenConnection();
            var id = await conn.ExecuteScalarAsync<long>(sql, customer);
            customer.Id = (int)id;
            return customer;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            const string sql = "SELECT * FROM customers ORDER BY createdAt DESC";
            using var conn = OpenConnection();
            return await conn.QueryAsync<Customer>(sql);
        }

        /// <inheritdoc />
        public async Task<Customer?> GetByIdAsync(int id)
        {
            const string sql = "SELECT * FROM customers WHERE id = @Id";
            using var conn = OpenConnection();
            return await conn.QuerySingleOrDefaultAsync<Customer>(sql, new { Id = id });
        }

        /// <inheritdoc />
        public async Task<int> CountAsync()
        {
            const string sql = "SELECT COUNT(*) FROM customers";
            using var conn = OpenConnection();
            return await conn.ExecuteScalarAsync<int>(sql);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetPagedAsync(int pageIndex, int pageSize)
        {
            const string sql = @"
                SELECT * FROM customers
                ORDER BY createdAt DESC
                LIMIT @PageSize OFFSET @Offset";

            using var conn = OpenConnection();
            return await conn.QueryAsync<Customer>(sql, new
            {
                PageSize = pageSize,
                Offset = pageIndex * pageSize
            });
        }
    }
}
