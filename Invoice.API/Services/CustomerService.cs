using Invoice.API.Data;
using Invoice.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Services;

public class CustomerService(InvoiceContext context) : IAsyncCustomerService
{
    private readonly InvoiceContext _context = context;

    public async Task<Customer> AddCustomerAsync(Customer customer)
    {
        var result = await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        
        return result.Entity;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        var result = _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
        
        return result.Entity;
    }

    public async Task<Customer> DeleteCustomerAsync(Guid id)
    {
        var result = await _context.Customers.FindAsync(id);
        _context.Customers.Remove(result!);
        await _context.SaveChangesAsync();
        
        return result!;
    }

    public async Task<Customer> SoftDeleteCustomerAsync(Guid id)
    {
        var result = await _context.Customers.FindAsync(id);
        result!.IsDeleted = true;
        _context.Customers.Update(result);
        await _context.SaveChangesAsync();
        
        return result;
    }

    public async Task<List<Customer>> GetCustomersAsync(string? filterOn, string? filterQuery, string? sortBy,
        bool isAscending = true,
        int pageNumber = 1, int pageSize = 100)
    {
        var skip = (pageNumber - 1) * pageSize;
        
        return await _context.Customers.Skip(skip).Take(pageSize).ToListAsync();
    }

    public async Task<Customer> GetCustomerAsync(Guid id)
    {
        return (await _context.Customers.FindAsync(id))!;
    }
}