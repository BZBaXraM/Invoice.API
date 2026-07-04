namespace Invoice.API.Controllers;

/// <summary>
/// Controller for invoice/customer/service statistics reports.
/// </summary>
[Authorize]
[Route("api/reports")]
[ApiController]
public class ReportsController(IReportService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Gets per-customer invoice count and total sum for the given period.
    /// </summary>
    [HttpGet("customers")]
    public async Task<ActionResult<ResponseModel<List<CustomerStatResponse>>>> GetCustomerStats(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var result = await service.GetCustomerStatsAsync(currentUserService.UserId!.Value, from, to);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets per-service invoice count and total sum for the given period.
    /// </summary>
    [HttpGet("services")]
    public async Task<ActionResult<ResponseModel<List<ServiceStatResponse>>>> GetServiceStats(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var result = await service.GetServiceStatsAsync(currentUserService.UserId!.Value, from, to);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets invoice counts grouped by status for the given period.
    /// </summary>
    [HttpGet("invoice-status")]
    public async Task<ActionResult<ResponseModel<List<InvoiceStatusStatResponse>>>> GetInvoiceStatusStats(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var result = await service.GetInvoiceStatusStatsAsync(currentUserService.UserId!.Value, from, to);
        return StatusCode(result.StatusCode, result);
    }
}
