namespace Invoice.Application.RepositoryContracts;

public interface IReportRepository
{
    Task<List<CustomerStatResponse>> GetCustomerStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
    Task<List<ServiceStatResponse>> GetServiceStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
    Task<List<InvoiceStatusStatResponse>> GetStatusStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
}
