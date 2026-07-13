namespace Invoice.API.Controllers;

/// <summary>
/// Controller for the user's change-history (audit log), scoped to their own data.
/// </summary>
[Authorize(Policy = AuthPolicies.NotAdmin)]
[Route("api/audit")]
[ApiController]
public class AuditController(IAuditService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Gets a paginated list of audit log entries, optionally filtered by entity type/id.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResult<AuditLogResponse>>>> GetAuditLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? entityType = null,
        [FromQuery] Guid? entityId = null)
    {
        var result = await service.GetListAsync(currentUserService.UserId!.Value, pageNumber, pageSize, entityType, entityId);
        return StatusCode(result.StatusCode, result);
    }
}
