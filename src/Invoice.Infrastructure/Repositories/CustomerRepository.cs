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
        bool sortDescending,
        bool includeArchived = false)
    {
        var query = context.Customers.Where(c => c.UserId == ownerUserId);

        if (!includeArchived)
        {
            query = query.Where(c => c.DeletedAt == null);
        }

        if (!string.IsNullOrWhiteSpace(nameFilter))
        {
            query = query.Where(c =>
                c.FirstName.Contains(nameFilter) ||
                c.LastName.Contains(nameFilter) ||
                (c.CompanyName != null && c.CompanyName.Contains(nameFilter)));
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "email" => sortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "createdat" => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
            "companyname" => sortDescending
                ? query.OrderByDescending(c => c.CompanyName)
                : query.OrderBy(c => c.CompanyName),
            _ => sortDescending
                ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
                : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName)
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
