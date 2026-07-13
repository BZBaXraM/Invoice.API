namespace Invoice.API.Controllers;

/// <summary>
/// Controller for managing invoices.
/// </summary>
[Authorize(Policy = AuthPolicies.NotAdmin)]
[Route("api/invoices")]
[ApiController]
public class InvoiceController(
    IInvoiceService service,
    IPaymentService paymentService,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Creates a new invoice.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseModel<InvoiceResponse>>> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var result = await service.CreateAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates an existing invoice. Only allowed while the invoice is still <c>Created</c>.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResponseModel<InvoiceResponse>>> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var result = await service.UpdateAsync(currentUserService.UserId!.Value, id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates the status of an invoice.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ResponseModel>> UpdateInvoiceStatus(Guid id, [FromBody] UpdateInvoiceStatusRequest request)
    {
        var result = await service.UpdateStatusAsync(currentUserService.UserId!.Value, id, request.Status);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets an invoice by its id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseModel<InvoiceResponse>>> GetInvoiceById(Guid id)
    {
        var result = await service.GetByIdAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets a paginated, filtered, sorted list of invoices.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResult<InvoiceResponse>>>> GetInvoices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? customerId = null,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var result = await service.GetListAsync(currentUserService.UserId!.Value, pageNumber, pageSize, customerId,
            status, sortBy, sortDescending);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes an invoice (hard delete). Only allowed while the invoice is still <c>Created</c>.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResponseModel>> DeleteInvoice(Guid id)
    {
        var result = await service.DeleteAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Archives an invoice (soft delete).
    /// </summary>
    [HttpPut("{id:guid}/archive")]
    public async Task<ActionResult<ResponseModel>> ArchiveInvoice(Guid id)
    {
        var result = await service.ArchiveAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Exports an invoice to a PDF file.
    /// </summary>
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> ExportInvoiceToPdf(Guid id)
    {
        var result = await service.ExportToPdfAsync(currentUserService.UserId!.Value, id);
        if (!result.IsSucceeded)
        {
            return StatusCode(result.StatusCode, result);
        }

        return File(result.Data!.Content, "application/pdf", result.Data.FileName);
    }

    /// <summary>
    /// Records a payment against an invoice and returns the refreshed invoice.
    /// </summary>
    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<ResponseModel<InvoiceResponse>>> AddPayment(Guid id, [FromBody] CreatePaymentRequest request)
    {
        var result = await paymentService.AddAsync(currentUserService.UserId!.Value, id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Lists the payments recorded against an invoice.
    /// </summary>
    [HttpGet("{id:guid}/payments")]
    public async Task<ActionResult<ResponseModel<List<PaymentResponse>>>> GetPayments(Guid id)
    {
        var result = await paymentService.GetListAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes a recorded payment and returns the refreshed invoice.
    /// </summary>
    [HttpDelete("{id:guid}/payments/{paymentId:guid}")]
    public async Task<ActionResult<ResponseModel<InvoiceResponse>>> DeletePayment(Guid id, Guid paymentId)
    {
        var result = await paymentService.DeleteAsync(currentUserService.UserId!.Value, id, paymentId);
        return StatusCode(result.StatusCode, result);
    }
}
