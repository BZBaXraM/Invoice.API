namespace Invoice.Application.Services;

public class ReportService(IUnitOfWork uow) : IReportService
{
    public async Task<ResponseModel<List<CustomerStatResponse>>> GetCustomerStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        var stats = await uow.ReportRepository.GetCustomerStatsAsync(ownerUserId, from, to);
        return ResponseModel.Success(stats);
    }

    public async Task<ResponseModel<List<ServiceStatResponse>>> GetServiceStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        var stats = await uow.ReportRepository.GetServiceStatsAsync(ownerUserId, from, to);
        return ResponseModel.Success(stats);
    }

    public async Task<ResponseModel<List<InvoiceStatusStatResponse>>> GetInvoiceStatusStatsAsync(Guid ownerUserId, DateTimeOffset from, DateTimeOffset to)
    {
        var stats = await uow.ReportRepository.GetStatusStatsAsync(ownerUserId, from, to);
        return ResponseModel.Success(stats);
    }
}
