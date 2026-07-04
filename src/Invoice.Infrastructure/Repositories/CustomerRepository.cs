using Invoice.Application.RepositoryContracts;
using Invoice.Domain.Entities;
using Invoice.Domain.Enums;
using Invoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Repositories;

public class CustomerRepository(InvoiceDbContext context) : ICustomerRepository
{
    public Customer AddCustomer(Customer customer)
    {
        context.Customers.Add(customer);
        return customer;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, Guid ownerUserId) =>
        await context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.UserId == ownerUserId);

    public async Task<(List<Customer> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? nameFilter,
        string? sortBy,
        bool sortDescending)
    {
        var query = context.Customers
            .Where(c => c.UserId == ownerUserId && c.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(c => c.Name.Contains(nameFilter));
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "email" => sortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "createdat" => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            _ => sortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<bool> HasSentInvoicesAsync(Guid customerId) =>
        await context.Invoices.AnyAsync(i => i.CustomerId == customerId && i.Status != InvoiceStatus.Created);

    public void RemoveCustomer(Customer customer) => context.Customers.Remove(customer);
}
