namespace Invoice.Infrastructure.Repositories;

public class ReportRepository(InvoiceDbContext context) : IReportRepository
{
    public async Task<List<CustomerStatResponse>> GetCustomerStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        var invoices = InRange(ownerUserId, from, to);

        return await invoices
            .Join(context.Customers.Where(c => c.DeletedAt == null), i => i.CustomerId, c => c.Id,
                (i, c) => new { i, c })
            .GroupBy(x => new { x.c.Id, x.c.Name })
            .Select(g => new CustomerStatResponse
            {
                CustomerId = g.Key.Id,
                CustomerName = g.Key.Name,
                InvoiceCount = g.Count(),
                TotalSum = g.Sum(x => x.i.TotalSum)
            })
            .ToListAsync();
    }

    public async Task<List<ServiceStatResponse>> GetServiceStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        var invoiceIds = InRange(ownerUserId, from, to).Select(i => i.Id);

        return await context.InvoiceRows
            .Where(r => invoiceIds.Contains(r.InvoiceId))
            .GroupBy(r => r.Service)
            .Select(g => new ServiceStatResponse
            {
                Service = g.Key,
                InvoiceCount = g.Select(r => r.InvoiceId).Distinct().Count(),
                TotalSum = g.Sum(r => r.Sum)
            })
            .ToListAsync();
    }

    public async Task<List<InvoiceStatusStatResponse>> GetStatusStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        return await InRange(ownerUserId, from, to)
            .GroupBy(i => i.Status)
            .Select(g => new InvoiceStatusStatResponse
            {
                Status = g.Key,
                Count = g.Count()
            })
            .ToListAsync();
    }

    private IQueryable<Domain.Entities.Invoice> InRange(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to) =>
        context.Invoices.Where(i =>
            i.UserId == ownerUserId &&
            i.DeletedAt == null &&
            i.StartDate >= from &&
            i.EndDate <= to);
}
