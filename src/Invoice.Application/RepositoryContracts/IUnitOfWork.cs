namespace Invoice.Application.RepositoryContracts;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    ICustomerRepository CustomerRepository { get; }
    IInvoiceRepository InvoiceRepository { get; }
    IReportRepository ReportRepository { get; }

    Task<bool> CommitAsync(CancellationToken cancellationToken = default);
}
