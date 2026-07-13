using Invoice.Domain.Exceptions;
using Npgsql;

namespace Invoice.Infrastructure.Repositories;

public class UnitOfWork(InvoiceDbContext context) : IUnitOfWork
{
    public IUserRepository UserRepository => field ??= new UserRepository(context);
    public ICustomerRepository CustomerRepository => field ??= new CustomerRepository(context);
    public IInvoiceRepository InvoiceRepository => field ??= new InvoiceRepository(context);
    public IReportRepository ReportRepository => field ??= new ReportRepository(context);
    public ICompanyProfileRepository CompanyProfileRepository => field ??= new CompanyProfileRepository(context);
    public IPaymentRepository PaymentRepository => field ??= new PaymentRepository(context);
    public IRecurringInvoiceRepository RecurringInvoiceRepository => field ??= new RecurringInvoiceRepository(context);
    public IAuditRepository AuditRepository => field ??= new AuditRepository(context);
    public IBackupRepository BackupRepository => field ??= new BackupRepository(context);

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg)
        {
            throw new UniqueConstraintViolationException(pg.ConstraintName, ex);
        }
    }
}