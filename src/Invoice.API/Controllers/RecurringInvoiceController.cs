namespace Invoice.API.Controllers;

/// <summary>
/// Controller for recurring invoice templates.
/// </summary>
[Authorize(Policy = AuthPolicies.NotAdmin)]
[Route("api/recurring-invoices")]
[ApiController]
public class RecurringInvoiceController(IRecurringInvoiceService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Creates a new recurring invoice template.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseModel<RecurringInvoiceResponse>>> CreateRecurringInvoice(
        [FromBody] CreateRecurringInvoiceRequest request)
    {
        var result = await service.CreateAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates an existing recurring invoice template.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResponseModel<RecurringInvoiceResponse>>> UpdateRecurringInvoice(
        Guid id, [FromBody] UpdateRecurringInvoiceRequest request)
    {
        var result = await service.UpdateAsync(currentUserService.UserId!.Value, id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets a recurring invoice template by its id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseModel<RecurringInvoiceResponse>>> GetRecurringInvoiceById(Guid id)
    {
        var result = await service.GetByIdAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets a paginated list of recurring invoice templates.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResult<RecurringInvoiceResponse>>>> GetRecurringInvoices(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await service.GetListAsync(currentUserService.UserId!.Value, pageNumber, pageSize);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Toggles a template between active and paused.
    /// </summary>
    [HttpPut("{id:guid}/toggle")]
    public async Task<ActionResult<ResponseModel<RecurringInvoiceResponse>>> ToggleRecurringInvoice(Guid id)
    {
        var result = await service.ToggleActiveAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes a recurring invoice template. Already-generated invoices are not affected.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResponseModel>> DeleteRecurringInvoice(Guid id)
    {
        var result = await service.DeleteAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }
}
