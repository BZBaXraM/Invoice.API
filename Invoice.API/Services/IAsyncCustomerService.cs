using Invoice.API.Models;

namespace Invoice.API.Services;

public interface IAsyncCustomerService
{
    Task<Customer> AddCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task<Customer> DeleteCustomerAsync(Guid id);
    Task<Customer> SoftDeleteCustomerAsync(Guid id);
    Task<List<Customer>> GetCustomersAsync(string? filterOn, string? filterQuery,
        string? sortBy, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
    Task<Customer> GetCustomerAsync(Guid id);
}