// using System.Text;
// using Invoice.API.Data;
// using Invoice.API.Models;
// using Invoice.API.Services;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using NReco.PdfGenerator;
//
// namespace Invoice.API.Controllers;
//
// [Route("api/[controller]")]
// [ApiController]
// public class ReportController(InvoiceContext context, IAsyncInvoiceService service) : ControllerBase
// {
//     [HttpGet("download/{id:guid}")]
//     Task<IActionResult> DownloadInvoiceAsync(Guid id)
//     {
//         var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
//
//         if (invoice == null)
//         {
//             return NotFound();
//         }
//
//         var htmlContent = await GenerateHtmlContentAsync(invoice);
//         var pdfContent = GeneratePdfContent(htmlContent);
//
//         return File(pdfContent, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
//     }
//
//     private async Task<string> GenerateHtmlContentAsync(InvoiceRow invoice)
//     {
//         var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == invoice.InvoiceId);
//
//         if (customer == null)
//         {
//             return null!;
//         }
//
//         var invoiceRows = await context.InvoiceRows.Where(ir => ir.InvoiceId == invoice.Id).ToListAsync();
//
//         var sb = new StringBuilder();
//         sb.Append(@$"
//             <html>
//                 <head>
//                     <style>
//                         table, th, td {{
//                             border: 1px solid black;
//                             border-collapse: collapse;
//                         }}
//                         th, td {{
//                             padding: 5px;
//                             text-align: left;
//                         }}
//                     </style>
//                 </head>
//                 <body>
//                     <h2>Invoice</h2>
//                     <table style=""width:100%"">
//                         <tr>
//                             <th>Invoice Number</th>
//                             <th>Invoice Date</th>
//                             <th>Due Date</th>
//                             <th>Customer Name</th>
//                             <th>Customer Address</th>
//                             <th>Customer Email</th>
//                             <th>Customer Phone</th>
//                         </tr>
//                         <tr>
//                     
//                             <td>{customer.Name}</td>
//                             <td>{customer.Address}</td>
//                             <td>{customer.Email}</td>
//                             <td>{customer.Phone}</td>
//                         </tr>
//                     </table>
//                     <br />
//                     <table style=""width:100%"">
//                         <tr>
//                             <th>Service</th>
//                             <th>Quantity</th>
//                             <th>Rate</th>
//                             <th>Amount</th>
//                         </tr>");
//
//         foreach (var invoiceRow in invoiceRows)
//         {
//             sb.Append(@$"
//                         <tr>
//                             <td>{invoiceRow.Service}</td>
//                             <td>{invoiceRow.Quantity}</td>
//                             <td>{invoiceRow.Rate}</td>
//                         </tr>");
//         }
//
//         sb.Append(@$"
//                         <tr>
//                             <td></td>
//                             <td></td>
//                             <td>Total</td>
//                             <td>{invoiceRows.Sum(ir => ir.Sum)}</td
//                         </tr>
//                     </table>
//                 </body>
//             </html>");
//
//         return sb.ToString();
//     }
//
//     private byte[] GeneratePdfContent(string htmlContent)
//     {
//         var converter = new HtmlToPdfConverter();
//         var doc = converter.ConvertHtmlString(htmlContent);
//         var pdfBytes = doc.Save();
//         doc.Close();
//         return pdfBytes;
//     }
//
//     [HttpGet("download")]
//     async Task<IActionResult> DownloadInvoicesAsync([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
//         [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
//         [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
//     {
//         var invoices = await service.GetInvoicesAsync(filterOn, filterQuery, sortBy,
//             isAscending ?? true,
//             pageNumber,
//             pageSize);
//
//         var htmlContent = await GenerateHtmlContentAsync(invoices);
//         var pdfContent = GeneratePdfContent(htmlContent);
//
//         return File(pdfContent, "application/pdf", $"Invoices.pdf");
//     }
//
//     private async Task<string> GenerateHtmlContentAsync(List<InvoiceRow> invoices)
//     {
//         var sb = new StringBuilder();
//         sb.Append(@$"
//             <html>
//                 <head>
//                     <style>
//                         table, th, td {{
//                             border: 1px solid black;
//                             border-collapse: collapse;
//                         }}
//                         th, td {{
//                             padding: 5px;
//                             text-align: left;
//                         }}
//                     </style>
//                 </head>
//                 <body>
//                     <h2>Invoices</h2>
//                     <table style=""width:100%"">
//                         <tr>
//                             <th>Invoice Number</th>
//                             <th>Invoice Date</th>
//                             <th>Due Date</th>
//                             <th>Customer Name</th>
//                             <th>Customer Address</th>
//                             <th>Customer Email</th>
//                             <th>Customer Phone</th>
//                         </tr>");
//
//         foreach (var invoice in invoices)
//         {
//             var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == invoice.InvoiceId);
//
//             if (customer == null)
//             {
//                 return null!;
//             }
//
//             sb.Append(@$"
//                         <tr> 
//                             <td>{customer.Name}</td>
//                             <td>{customer.Address}</td>
//                             <td>{customer.Email}</td>
//                             <td>{customer.Phone}</td>
//                         </tr>");
//         }
//
//         sb.Append(@$"
//                     </table>
//                 </body>
//             </html>");
//
//         return sb.ToString();
//     }
//
//     [HttpGet("download/{id:guid}/send")]
//     async Task<IActionResult> SendInvoiceAsync(Guid id)
//     {
//         var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
//
//         if (invoice == null)
//         {
//             return NotFound();
//         }
//
//         var htmlContent = await GenerateHtmlContentAsync(invoice);
//         var pdfContent = GeneratePdfContent(htmlContent);
//
//         var customer = await context.Customers.FirstOrDefaultAsync(c => c.Id == invoice.InvoiceId);
//
//         if (customer == null)
//         {
//             return NotFound();
//         }
//
//         var email = new MimeMessage();
//         email.From.Add(MailboxAddress.Parse("   "));
//         email.To.Add(MailboxAddress.Parse(customer.Email));
//     }