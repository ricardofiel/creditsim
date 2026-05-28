using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CreditSim.Core.Models;
using Newtonsoft.Json;

namespace CreditSim.Data.Repositories
{
    /// <summary>
    /// File-backed implementation of <see cref="ICustomerRepository"/>.
    /// Persists all customers as a single JSON array on disk.
    /// </summary>
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _filePath;
        private readonly object _sync = new object();

        public CustomerRepository(string filePath)
        {
            _filePath = filePath;
        }

        private List<Customer> Load()
        {
            if (!File.Exists(_filePath))
                return new List<Customer>();

            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
                return new List<Customer>();

            return JsonConvert.DeserializeObject<List<Customer>>(json) ?? new List<Customer>();
        }

        private void Save(List<Customer> customers)
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonConvert.SerializeObject(customers, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        /// <inheritdoc />
        public Task<Customer> InsertAsync(Customer customer)
        {
            lock (_sync)
            {
                var all = Load();
                customer.Id = all.Count == 0 ? 1 : all.Max(c => c.Id) + 1;
                if (customer.CreatedAt == default)
                    customer.CreatedAt = DateTime.UtcNow;
                all.Add(customer);
                Save(all);
                return Task.FromResult(customer);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<Customer>> GetAllAsync()
        {
            lock (_sync)
            {
                IEnumerable<Customer> result = Load()
                    .OrderByDescending(c => c.CreatedAt)
                    .ToList();
                return Task.FromResult(result);
            }
        }

        /// <inheritdoc />
        public Task<Customer?> GetByIdAsync(int id)
        {
            lock (_sync)
            {
                var match = Load().FirstOrDefault(c => c.Id == id);
                return Task.FromResult<Customer?>(match);
            }
        }

        /// <inheritdoc />
        public Task<int> CountAsync()
        {
            lock (_sync)
            {
                return Task.FromResult(Load().Count);
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<Customer>> GetPagedAsync(int pageIndex, int pageSize)
        {
            lock (_sync)
            {
                IEnumerable<Customer> result = Load()
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize)
                    .ToList();
                return Task.FromResult(result);
            }
        }
    }
}

