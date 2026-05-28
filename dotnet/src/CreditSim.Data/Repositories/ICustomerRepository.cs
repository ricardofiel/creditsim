using System.Collections.Generic;
using System.Threading.Tasks;
using CreditSim.Core.Models;

namespace CreditSim.Data.Repositories
{
    public interface ICustomerRepository
    {
        /// <summary>Inserts a new customer record and returns it with the generated Id.</summary>
        Task<Customer> InsertAsync(Customer customer);

        /// <summary>Returns all customer records ordered by createdAt DESC.</summary>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>Returns a single customer by primary key, or null if not found.</summary>
        Task<Customer?> GetByIdAsync(int id);

        /// <summary>Returns the total number of customer records.</summary>
        Task<int> CountAsync();

        /// <summary>
        /// Returns a page of customers ordered by createdAt DESC.
        /// <paramref name="pageIndex"/> is 0-based.
        /// </summary>
        Task<IEnumerable<Customer>> GetPagedAsync(int pageIndex, int pageSize);
    }
}
