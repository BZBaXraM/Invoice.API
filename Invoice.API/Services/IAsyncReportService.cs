using Invoice.API.Models;
using Invoice.API.Models.Enum;

namespace Invoice.API.Services;

public interface IAsyncReportService
{
    Task<List<Customer>> GetCustomerStatisticsAsync(DateTimeOffset from, DateTimeOffset to);
    Task<List<InvoiceRow>> GetWorkStatisticsAsync(DateTimeOffset from, DateTimeOffset to);
    Task<List<InvoiceStatus>> GetInvoiceStatisticsAsync(DateTimeOffset from, DateTimeOffset to);
    
}