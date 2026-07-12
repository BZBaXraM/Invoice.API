namespace Invoice.Application.ServiceContracts;

public interface IAdminService
{
    Task<ResponseModel<PagedResult<AdminUserResponse>>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchFilter);

    Task<ResponseModel> DisableUserAsync(Guid currentAdminId, Guid userId);
    Task<ResponseModel> EnableUserAsync(Guid currentAdminId, Guid userId);
    Task<ResponseModel> DeleteUserAsync(Guid currentAdminId, Guid userId);
}
