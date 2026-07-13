namespace Invoice.Application.RepositoryContracts;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IReportRepository ReportRepository { get; }
    ICompanyProfileRepository CompanyProfileRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IRecurringInvoiceRepository RecurringInvoiceRepository { get; }
    IAuditRepository AuditRepository { get; }
    IBackupRepository BackupRepository { get; }

    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}
