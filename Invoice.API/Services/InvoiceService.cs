using System.Globalization;
using Invoice.API.Data;
using Microsoft.EntityFrameworkCore;
using NReco.PdfGenerator;

namespace Invoice.API.Services;

public class InvoiceService(InvoiceContext context) : IAsyncInvoiceService
{
    public async Task<Models.Invoice> AddInvoiceAsync(Models.Invoice invoice)
    {
        var result = await context.Invoices.AddAsync(invoice);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Models.Invoice> UpdateInvoiceAsync(Models.Invoice invoice)
    {
        var result = context.Invoices.Update(invoice);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Models.Invoice> SendInvoiceAsync(Guid id)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return null!;
        }

        invoice.IsSent = true;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        var result = context.Invoices.Update(invoice);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Models.Invoice> SoftDeleteInvoiceAsync(Guid id)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return null!;
        }

        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;
        var result = context.Invoices.Update(invoice);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Models.Invoice> DeleteInvoiceAsync(Guid id)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return null!;
        }

        var result = context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<List<Models.Invoice>> GetInvoicesAsync(string? filterOn, string? filterQuery, string? sortBy,
        bool isAscending = true,
        int pageNumber = 1, int pageSize = 100)
    {
        var skip = (pageNumber - 1) * pageSize;
        return await context.Invoices
            .Where(i => !i.IsDeleted)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Models.Invoice> GetInvoiceAsync(Guid id)
    {
        return (await context.Invoices.FirstOrDefaultAsync(i => i.Id == id))!;
    }

    public async Task<byte[]> DownloadInvoiceAsync(Guid id)
    {
        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
        {
            return null!;
        }

        var invoiceTemplate = await File.ReadAllTextAsync("Templates/InvoiceTemplate.html");
        var invoiceHtml = invoiceTemplate
            .Replace("{{InvoiceId}}", invoice.Id.ToString())
            .Replace("{{DueDate}}", invoice.StartDate.ToString("dd/MM/yyyy"))
            .Replace("{{InvoiceDate}}", invoice.CreatedAt.ToString("dd/MM/yyyy"))
            .Replace("{{TotalSum}}", invoice.TotalSum.ToString(CultureInfo.InvariantCulture))
            .Replace("{{IsSent}}", invoice.IsSent ? "Yes" : "No");

        var converter = new HtmlToPdfConverter();
        var pdfBytes = converter.GeneratePdf(invoiceHtml, "Invoice");

        return pdfBytes;
    }
}