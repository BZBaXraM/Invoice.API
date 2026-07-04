using Invoice.Application.RepositoryContracts;
using Invoice.Infrastructure.Data;

namespace Invoice.Infrastructure.Repositories;

public class UnitOfWork(InvoiceDbContext context) : IUnitOfWork
{
    public IUserRepository UserRepository => field ??= new UserRepository(context);
    public ICustomerRepository CustomerRepository => field ??= new CustomerRepository(context);
    public IInvoiceRepository InvoiceRepository => field ??= new InvoiceRepository(context);
    public IReportRepository ReportRepository => field ??= new ReportRepository(context);

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken) > 0;
}