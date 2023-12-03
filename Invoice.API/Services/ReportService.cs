using Invoice.API.Data;
using Invoice.API.Models;
using Invoice.API.Models.Enum;

namespace Invoice.API.Services;

public class ReportService(InvoiceContext context) : IAsyncReportService
{
    public async Task<List<Customer>> GetCustomerStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
    {
        throw new NotImplementedException();
    }

    public async Task<List<InvoiceRow>> GetWorkStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
    {
        throw new NotImplementedException();
    }

    public async Task<List<InvoiceStatus>> GetInvoiceStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
    {
        throw new NotImplementedException();
    }
}