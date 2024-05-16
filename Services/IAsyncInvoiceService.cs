using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;

namespace InvoiceManager.API.Services;

/// <summary>
/// Service for managing invoices asynchronously.
/// </summary>
public interface IAsyncInvoiceService
{
    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceDto dto);

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<InvoiceDto> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto);

    /// <summary>
    ///  Gets an invoice by its ID. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<InvoiceDto> GetInvoiceByIdAsync(int id);

    /// <summary>
    /// Gets a list of invoices.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<List<InvoiceDto>> GetInvoicesAsync(int pageNumber, int pageSize);
    /// <summary>
    /// Deletes an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DeleteInvoiceAsync(int id);
    /// <summary>
    /// Archives an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task ArchiveInvoiceAsync(int id);
    /// <summary>
    /// Updates the status of an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    Task UpdateInvoiceStatusAsync(int id, InvoiceStatus status);
    /// <summary>
    /// Exports an invoice to a PDF file.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<string> ExportInvoiceToPdfAsync(int id, CancellationToken cancellationToken = default);
}