namespace Invoice.API.Controllers;

/// <summary>
/// Controller for customer management.
/// </summary>
[Authorize]
[Route("api/customers")]
[ApiController]
public class CustomerController(ICustomerService service, ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ResponseModel<CustomerResponse>>> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var result = await service.CreateAsync(currentUserService.UserId!.Value, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ResponseModel<CustomerResponse>>> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        var result = await service.UpdateAsync(currentUserService.UserId!.Value, id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets a customer by its id.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponseModel<CustomerResponse>>> GetCustomerById(Guid id)
    {
        var result = await service.GetByIdAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gets a paginated, filtered, sorted list of customers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ResponseModel<PagedResult<CustomerResponse>>>> GetCustomers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? nameFilter = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] bool includeArchived = false)
    {
        var result = await service.GetListAsync(currentUserService.UserId!.Value, pageNumber, pageSize, nameFilter,
            sortBy, sortDescending, includeArchived);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Deletes a customer (hard delete). Only allowed if no invoice has ever been sent to them.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ResponseModel>> DeleteCustomer(Guid id)
    {
        var result = await service.DeleteAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Archives a customer (soft delete).
    /// </summary>
    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<ResponseModel>> ArchiveCustomer(Guid id)
    {
        var result = await service.ArchiveAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Restores a previously archived customer.
    /// </summary>
    [HttpPost("{id:guid}/unarchive")]
    public async Task<ActionResult<ResponseModel>> UnarchiveCustomer(Guid id)
    {
        var result = await service.UnarchiveAsync(currentUserService.UserId!.Value, id);
        return StatusCode(result.StatusCode, result);
    }
}
