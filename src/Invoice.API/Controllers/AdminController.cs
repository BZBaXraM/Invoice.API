namespace Invoice.API.Controllers;

/// <summary>
/// Controller for admin user management.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("api/admin")]
[ApiController]
public class AdminController(IAdminService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Gets a paginated list of all users, including those who never confirmed their email.
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<ResponseModel<PagedResult<AdminUserResponse>>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchFilter = null)
    {
        var result = await service.GetUsersAsync(pageNumber, pageSize, searchFilter);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Disables a user account: the user can no longer log in, refresh or call the API.
    /// </summary>
    [HttpPut("users/{id:guid}/disable")]
    public async Task<ActionResult<ResponseModel>> DisableUser(Guid id)
    {
        var result = await service.DisableUserAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Re-enables a previously disabled user account.
    /// </summary>
    [HttpPut("users/{id:guid}/enable")]
    public async Task<ActionResult<ResponseModel>> EnableUser(Guid id)
    {
        var result = await service.EnableUserAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes a user account (hard delete, cascades to their customers and invoices).
    /// </summary>
    [HttpDelete("users/{id:guid}")]
    public async Task<ActionResult<ResponseModel>> DeleteUser(Guid id)
    {
        var result = await service.DeleteUserAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }
}
