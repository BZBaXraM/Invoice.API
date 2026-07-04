namespace Invoice.Application.ServiceContracts;

public interface IReportService
{
    Task<ResponseModel<List<CustomerStatResponse>>> GetCustomerStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
    Task<ResponseModel<List<ServiceStatResponse>>> GetServiceStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
    Task<ResponseModel<List<InvoiceStatusStatResponse>>> GetInvoiceStatusStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to);
}
