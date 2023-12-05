// using Invoice.API.Data;
// using Invoice.API.Models;
// using Invoice.API.Models.Enum;
//
// namespace Invoice.API.Services;
//
// public class ReportService(InvoiceContext context) : IAsyncReportService
// {
//     public async Task<List<Customer>> GetCustomerStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
//     {
//         var customers = await context.Customers
//             .Include(c => c.Invoices)
//             .ThenInclude(i => i.InvoiceRows)
//             .Where(c => c.Invoices.Any(i => i.InvoiceDate >= from && i.InvoiceDate <= to))
//             .ToListAsync();
//     }
//
//     public async Task<List<InvoiceRow>> GetWorkStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
//     {
//        var invoiceRows = await context.InvoiceRows
//             .Include(ir => ir.Invoice)
//             .ThenInclude(i => i.Customer)
//             .Where(ir => ir.Invoice.InvoiceDate >= from && ir.Invoice.InvoiceDate <= to)
//             .ToListAsync();
//     }
//
//     public async Task<List<InvoiceStatus>> GetInvoiceStatisticsAsync(DateTimeOffset from, DateTimeOffset to)
//     {
//         var invoiceStatuses = await context.Invoices
//             .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
//             .GroupBy(i => i.Status)
//             .Select(g => new InvoiceStatus
//             {
//                 Status = g.Key,
//                 Count = g.Count()
//             })
//             .ToListAsync();
//     }
// }