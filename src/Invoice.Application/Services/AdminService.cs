namespace Invoice.Application.Services;

public class AdminService(IUnitOfWork uow) : IAdminService
{
    public async Task<ResponseModel<PagedResult<AdminUserResponse>>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchFilter)
    {
        var (items, totalCount) = await uow.UserRepository.GetPagedAsync(pageNumber, pageSize, searchFilter);

        return ResponseModel.Success(new PagedResult<AdminUserResponse>
        {
            Items = items.Select(u => u.ToAdminUserResponse()).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }

    public async Task<ResponseModel> DisableUserAsync(Guid currentAdminId, Guid userId)
    {
        if (userId == currentAdminId)
        {
            return ResponseModel.Failure("You cannot disable your own account", 400);
        }

        var user = await uow.UserRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return ResponseModel.Failure("User not found", 404);
        }

        if (user.Role == UserRole.Admin)
        {
            return ResponseModel.Failure("Cannot disable an admin account", 400);
        }

        if (!user.IsActive)
        {
            return ResponseModel.Failure("Account is already disabled", 409);
        }

        user.IsActive = false;
        user.RefreshToken = null;
        user.RefreshTokenExpireTime = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success("Account disabled successfully");
    }

    public async Task<ResponseModel> EnableUserAsync(Guid currentAdminId, Guid userId)
    {
        var user = await uow.UserRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return ResponseModel.Failure("User not found", 404);
        }

        if (user.IsActive)
        {
            return ResponseModel.Failure("Account is already active", 409);
        }

        user.IsActive = true;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await uow.CommitAsync();

        return ResponseModel.Success("Account enabled successfully");
    }

    public async Task<ResponseModel> DeleteUserAsync(Guid currentAdminId, Guid userId)
    {
        if (userId == currentAdminId)
        {
            return ResponseModel.Failure("You cannot delete your own account", 400);
        }

        var user = await uow.UserRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return ResponseModel.Failure("User not found", 404);
        }

        if (user.Role == UserRole.Admin)
        {
            return ResponseModel.Failure("Cannot delete an admin account", 400);
        }

        uow.UserRepository.RemoveUser(user);
        await uow.CommitAsync();

        return ResponseModel.Success("Account deleted successfully");
    }
}
