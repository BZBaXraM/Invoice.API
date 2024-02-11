using AutoMapper;
using Invoice.API.Data;
using Invoice.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace Invoice.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InvoiceController : ControllerBase
{
    private readonly IAsyncInvoiceService _invoiceService;
    private readonly IMapper _mapper;

    public InvoiceController(InvoiceContext context, IMapper mapper)
    {
        _invoiceService = new InvoiceService(context);
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> GetInvoices([FromQuery] string? filterOn, [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy, [FromQuery] bool? isAscending,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
    {
        var invoices = await _invoiceService.GetInvoicesAsync(filterOn, filterQuery, sortBy,
            isAscending ?? true,
            pageNumber,
            pageSize);
        
        return Ok(_mapper.Map<List<Models.Invoice>>(invoices));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceService.GetInvoiceAsync(id);
        return Ok(_mapper.Map<Models.Invoice>(invoice));
    }
    
    [HttpPost]
    public async Task<IActionResult> AddInvoiceAsync([FromBody] Models.Invoice invoice)
    {
        var newInvoice = await _invoiceService.AddInvoiceAsync(_mapper.Map<Invoice.API.Models.Invoice>(invoice));
        return CreatedAtAction(nameof(GetInvoices), new {id = newInvoice.Id}, _mapper.Map<Models.Invoice>(newInvoice));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateInvoiceAsync(Guid id, [FromBody] Models.Invoice invoice)
    {
        if (id != invoice.Id)
        {
            return BadRequest();
        }

        var updatedInvoice = await _invoiceService.UpdateInvoiceAsync(_mapper.Map<Invoice.API.Models.Invoice>(invoice));
        return Ok(_mapper.Map<Models.Invoice>(updatedInvoice));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceService.DeleteInvoiceAsync(id);
        return Ok(_mapper.Map<Models.Invoice>(invoice));
    }
    
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> SoftDeleteInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceService.SoftDeleteInvoiceAsync(id);
        return Ok(_mapper.Map<Models.Invoice>(invoice));
    }
    
    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> DownloadInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceService.DownloadInvoiceAsync(id);
        return File(invoice, "application/pdf");
    }
    
    [HttpPatch("{id:guid}/send")]
    public async Task<IActionResult> SendInvoiceAsync(Guid id)
    {
        var invoice = await _invoiceService.SendInvoiceAsync(id);
        return Ok(_mapper.Map<Models.Invoice>(invoice));
    }
}