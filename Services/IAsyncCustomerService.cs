using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;

namespace InvoiceManager.API.Services;

/// <summary>
/// Interface for the Customer Service
/// </summary>
public interface IAsyncCustomerService
{
    /// <summary>
    ///   Create a new customer
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);

    /// <summary>
    /// Update an existing customer
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    /// <summary>
    /// Get a customer by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<CustomerDto> GetCustomerByIdAsync(int id);
    /// <summary>
    /// Get a list of customers
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<List<CustomerDto>> GetCustomersAsync(int pageNumber, int pageSize);
    /// <summary>
    /// Delete a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteCustomerAsync(int id);
    /// <summary>
    /// Archive a customer
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task ArchiveCustomerAsync(int id);
}