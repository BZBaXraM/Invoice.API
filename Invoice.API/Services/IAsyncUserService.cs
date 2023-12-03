using Invoice.API.Models;

namespace Invoice.API.Services;

public interface IAsyncUserService
{
    Task<List<User>> GetUsersAsync(string? filterOn, string? filterQuery,
        string? sortBy, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
    Task<User> RegisterUserAsync(User user);
    Task<User> LoginUserAsync(User username);
    Task<User> UpdateUserAsync(User user);
    Task<User> ChangePasswordAsync(User user, string newPassword);
    Task<User> DeleteUserAsync(Guid id);
}