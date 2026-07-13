namespace Invoice.Infrastructure.Repositories;

public class InvoiceRepository(InvoiceDbContext context) : IInvoiceRepository
{
    public Domain.Entities.Invoice AddInvoice(Domain.Entities.Invoice invoice)
    {
        context.Invoices.Add(invoice);
        return invoice;
    }

    public InvoiceRow AddRow(InvoiceRow row)
    {
        context.InvoiceRows.Add(row);
        return row;
    }

    public async Task<Domain.Entities.Invoice?> GetByIdWithRowsAsync(Guid id, Guid ownerUserId) =>
        await context.Invoices
            .Include(i => i.Rows)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == ownerUserId);

    private static readonly InvoiceStatus[] OverdueEligibleStatuses =
    [
        InvoiceStatus.Sent,
        InvoiceStatus.Received,
        InvoiceStatus.PartiallyPaid
    ];

    public async Task<List<Domain.Entities.Invoice>> GetOverdueCandidatesAsync(DateTimeOffset now) =>
        await context.Invoices
            .Where(i => i.DeletedAt == null
                        && i.DueDate != null && i.DueDate < now
                        && OverdueEligibleStatuses.Contains(i.Status))
            .ToListAsync();

    public async Task<int> GetNextInvoiceNumberAsync(Guid ownerUserId)
    {
        var maxNumber = await context.Invoices
            .Where(i => i.UserId == ownerUserId)
            .MaxAsync(i => (int?)i.InvoiceNumber);
        return (maxNumber ?? 0) + 1;
    }

    public async Task<(List<Domain.Entities.Invoice> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        Guid? customerId,
        InvoiceStatus? status,
        string? sortBy,
        bool sortDescending)
    {
        var query = context.Invoices
            .Include(i => i.Rows)
            .Include(i => i.Payments)
            .Where(i => i.UserId == ownerUserId && i.DeletedAt == null);

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "totalsum" => sortDescending ? query.OrderByDescending(i => i.TotalSum) : query.OrderBy(i => i.TotalSum),
            "startdate" => sortDescending ? query.OrderByDescending(i => i.StartDate) : query.OrderBy(i => i.StartDate),
            _ => sortDescending ? query.OrderByDescending(i => i.CreatedAt) : query.OrderBy(i => i.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public void RemoveInvoice(Domain.Entities.Invoice invoice) => context.Invoices.Remove(invoice);
}
