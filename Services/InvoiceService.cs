using InvoiceManager.API.Data;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LicenseType = QuestPDF.Infrastructure.LicenseType;

namespace InvoiceManager.API.Services;

/// <summary>
/// Represents a service for managing invoices.
/// </summary>
/// <param name="context"></param>
public class InvoiceService(InvoiceDbContext context) : IAsyncInvoiceService
{
    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto dto)
    {
        var invoice = new Invoice
        {
            CustomerId = dto.CustomerId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Comment = dto.Comment,
            Status = InvoiceStatus.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Rows = dto.Rows.Select(r => new InvoiceRow
            {
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Quantity * r.Rate
            }).ToList()
        };

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);

        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();

        return new InvoiceDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Comment = invoice.Comment,
            Status = invoice.Status,
            TotalSum = invoice.TotalSum,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            Rows = invoice.Rows.Select(r => new InvoiceRowDto
            {
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Sum
            }).ToList()
        };
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<InvoiceDto> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
    {
        var invoice = await context.Invoices.Include(i => i.Rows).FirstOrDefaultAsync(i => i.Id == id);
        if (invoice is not { Status: InvoiceStatus.Created })
        {
            throw new KeyNotFoundException("Invoice not found or cannot be updated");
        }

        invoice.CustomerId = dto.CustomerId;
        invoice.StartDate = dto.StartDate;
        invoice.EndDate = dto.EndDate;
        invoice.Comment = dto.Comment;
        invoice.Status = dto.Status;
        invoice.UpdatedAt = DateTimeOffset.UtcNow;

        invoice.Rows.Clear();
        invoice.Rows.AddRange(dto.Rows.Select(r => new InvoiceRow
        {
            Service = r.Service,
            Quantity = r.Quantity,
            Rate = r.Rate,
            Sum = r.Quantity * r.Rate
        }).ToList());

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);

        await context.SaveChangesAsync();

        return new InvoiceDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Rows = invoice.Rows.Select(r => new InvoiceRowDto
            {
                Id = r.Id,
                InvoiceId = r.InvoiceId,
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Sum
            }).ToList(),
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    /// <summary>
    /// Retrieves an invoice by its unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<InvoiceDto> GetInvoiceByIdAsync(int id)
    {
        var invoice = await context.Invoices.Include(i => i.Rows).FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found");
        }

        return new InvoiceDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Rows = invoice.Rows.Select(r => new InvoiceRowDto
            {
                Id = r.Id,
                InvoiceId = r.InvoiceId,
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Sum
            }).ToList(),
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    /// <summary>
    /// Retrieves a list of invoices.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public async Task<List<InvoiceDto>> GetInvoicesAsync(int pageNumber, int pageSize)
    {
        var invoices = await context.Invoices
            .Include(i => i.Rows)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return invoices.Select(invoice => new InvoiceDto
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Rows = invoice.Rows.Select(r => new InvoiceRowDto
            {
                Id = r.Id,
                InvoiceId = r.InvoiceId,
                Service = r.Service,
                Quantity = r.Quantity,
                Rate = r.Rate,
                Sum = r.Sum
            }).ToList(),
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        }).ToList();
    }

    /// <summary>
    /// Deletes an invoice by its unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteInvoiceAsync(int id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice is not { Status: InvoiceStatus.Created })
        {
            throw new KeyNotFoundException("Invoice not found or cannot be deleted");
        }

        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Archives an invoice by its unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task ArchiveInvoiceAsync(int id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice is not { Status: InvoiceStatus.Created })
        {
            throw new KeyNotFoundException("Invoice not found or cannot be archived");
        }

        invoice.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the status of an invoice by its unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task UpdateInvoiceStatusAsync(int id, InvoiceStatus status)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found");
        }

        invoice.Status = status;
        await context.SaveChangesAsync();
    }


    /// <summary>
    /// Exports an invoice to a PDF file.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<string> ExportInvoiceToPdfAsync(int id, CancellationToken cancellationToken = default)
    {
        var invoice = await context.Invoices.Include(i => i.Rows)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken: cancellationToken);
        if (invoice == null)
        {
            throw new KeyNotFoundException("Invoice not found");
        }

        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A1);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(14));

                page.Header()
                    .Text("Invoice Details")
                    .SemiBold()
                    .FontSize(20)
                    .FontColor(Colors.Black);

                page.Content().Column(stack =>
                {
                    stack.Item().Text($"Invoice ID: {invoice.Id}");
                    stack.Item().Text($"Customer ID: {invoice.CustomerId}");
                    stack.Item().Text($"Start Date: {invoice.StartDate}");
                    stack.Item().Text($"End Date: {invoice.EndDate}");
                    stack.Item().Text($"Total Sum: {invoice.TotalSum}");
                    stack.Item().Text($"Comment: {invoice.Comment}");
                    stack.Item().Text($"Status: {invoice.Status}");
                    stack.Item().Text($"Created At: {invoice.CreatedAt}");
                    stack.Item().Text($"Updated At: {invoice.UpdatedAt}");
                    stack.Item().Text($"Deleted At: {invoice.DeletedAt}");
                    // stack.Item().Text($"Rows: {invoice.Rows.Select(x=> x.InvoiceId)}").SemiBold();
                    foreach (var item in invoice.Rows)
                    {
                        stack.Item().Text($"Service: {item.Service}");
                        stack.Item().Text($"Quantity: {item.Quantity}");
                        stack.Item().Text($"Rate: {item.Rate}");
                        stack.Item().Text($"Sum: {item.Sum}");
                    }
                });
            });
        });

        var pdf = document.GeneratePdf();
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdfs");
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, $"{invoice.Id}.pdf");
        await File.WriteAllBytesAsync(filePath, pdf, cancellationToken);

        return filePath;
    }
}