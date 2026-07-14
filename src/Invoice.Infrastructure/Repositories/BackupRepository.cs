namespace Invoice.Infrastructure.Repositories;

public class BackupRepository(InvoiceDbContext context) : IBackupRepository
{
    public async Task<UserDataGraph?> GetUserGraphAsync(Guid userId)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return null;
        }

        return new UserDataGraph
        {
            User = user,
            CompanyProfile = await context.CompanyProfiles.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId),
            Customers = await context.Customers.AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync(),
            Invoices = await context.Invoices.AsNoTracking()
                .Include(i => i.Rows)
                .Include(i => i.Payments)
                .Where(i => i.UserId == userId)
                .OrderBy(i => i.InvoiceNumber)
                .AsSplitQuery()
                .ToListAsync(),
            RecurringInvoices = await context.RecurringInvoices.AsNoTracking()
                .Include(r => r.Rows)
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync(),
            AuditLogs = await context.AuditLogs.AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync()
        };
    }

}
