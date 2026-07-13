namespace Invoice.Infrastructure.Repositories;

public class RecurringInvoiceRepository(InvoiceDbContext context) : IRecurringInvoiceRepository
{
    public RecurringInvoice AddRecurringInvoice(RecurringInvoice recurringInvoice)
    {
        context.RecurringInvoices.Add(recurringInvoice);
        return recurringInvoice;
    }

    public RecurringInvoiceRow AddRow(RecurringInvoiceRow row)
    {
        context.RecurringInvoiceRows.Add(row);
        return row;
    }

    public async Task<RecurringInvoice?> GetByIdWithRowsAsync(Guid id, Guid ownerUserId) =>
        await context.RecurringInvoices
            .Include(r => r.Rows)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == ownerUserId);

    public async Task<(List<RecurringInvoice> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize)
    {
        var query = context.RecurringInvoices
            .Include(r => r.Rows)
            .Where(r => r.UserId == ownerUserId)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<RecurringInvoice>> GetDueAsync(DateTimeOffset now) =>
        await context.RecurringInvoices
            .Include(r => r.Rows)
            .Where(r => r.IsActive && r.NextRunDate <= now)
            .ToListAsync();

    public void RemoveRecurringInvoice(RecurringInvoice recurringInvoice) =>
        context.RecurringInvoices.Remove(recurringInvoice);
}
