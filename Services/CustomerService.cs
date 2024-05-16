using InvoiceManager.API.Data;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Services;

/// <summary>
/// Customer service
/// </summary>
/// <param name="context"></param>
public class CustomerService(InvoiceDbContext context) : IAsyncCustomerService
{
    /// <summary>
    /// Database context
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        var customer = new Customer
        {
            Name = dto.Name,
            Address = dto.Address,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    /// <summary>
    /// Update customer
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto)
    {
        var customer = await context.Customers.FindAsync(id);

        if (customer is null)
        {
            throw new Exception("Customer not found");
        }

        customer.Name = dto.Name;
        customer.Address = dto.Address;
        customer.Email = dto.Email;
        customer.PhoneNumber = dto.PhoneNumber;
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync();

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    /// <summary>
    /// Get customer by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var customer = await context.Customers.FindAsync(id);

        if (customer is null)
        {
            throw new Exception("Customer not found");
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    /// <summary>
    /// Get customers
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<List<CustomerDto>> GetCustomersAsync(int pageNumber, int pageSize)
    {
        var customers = await context.Customers
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return customers.Select(customer => new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        }).ToList();
    }

    /// <summary>
    /// Delete customer
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="Exception"></exception>
    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await context.Customers.FindAsync(id);

        if (customer is null)
        {
            throw new Exception("Customer not found");
        }

        context.Customers.Remove(customer);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Archive customer
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="Exception"></exception>
    public async Task ArchiveCustomerAsync(int id)
    {
        var customer = await context.Customers.FindAsync(id);

        if (customer is null)
        {
            throw new Exception("Customer not found");
        }

        customer.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
    }
}