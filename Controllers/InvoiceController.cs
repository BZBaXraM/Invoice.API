using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;
using InvoiceManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceManager.API.Controllers;

/// <summary>
/// Controller for managing invoices.
/// </summary>
/// <param name="service"></param>
[Route("api/[controller]")]
[ApiController]
public class InvoiceController(IAsyncInvoiceService service) : ControllerBase
{
    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> CreateInvoice([FromBody] CreateInvoiceDto dto)
    {
        var invoice = await service.CreateInvoiceAsync(dto);
        return Ok(invoice);
    }

    /// <summary>
    /// Updates an existing invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<InvoiceDto>> UpdateInvoice(int id, [FromBody] UpdateInvoiceDto dto)
    {
        var invoice = await service.UpdateInvoiceAsync(id, dto);
        return Ok(invoice);
    }

    /// <summary>
    /// Gets an invoice by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetInvoiceById(int id)
    {
        var invoice = await service.GetInvoiceByIdAsync(id);
        return Ok(invoice);
    }

    /// <summary>
    /// Gets a list of invoices.
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<List<InvoiceDto>>> GetInvoices(int pageNumber, int pageSize)
    {
        var invoices = await service.GetInvoicesAsync(pageNumber, pageSize);
        return Ok(invoices);
    }

    /// <summary>
    /// Deletes an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteInvoice(int id)
    {
        await service.DeleteInvoiceAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Archives an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/archive")]
    public async Task<ActionResult> ArchiveInvoice(int id)
    {
        await service.ArchiveInvoiceAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Updates the status of an invoice.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult> UpdateInvoiceStatus(int id, [FromBody] InvoiceStatus status)
    {
        await service.UpdateInvoiceStatusAsync(id, status);
        return NoContent();
    }

    /// <summary>
    /// Exports an invoice to a PDF file.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id:int}/export")]
    public async Task<ActionResult> ExportInvoiceToPdf(int id)
    {
        var filePath = await service.ExportInvoiceToPdfAsync(id);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return new FileContentResult(fileBytes, "application/pdf")
        {
            FileDownloadName = Path.GetFileName(filePath)
        };
    }
}