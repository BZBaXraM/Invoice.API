using Invoice.API.Data;
using Invoice.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Invoice.API.Services;

public class UserService(InvoiceContext context) : IAsyncUserService
{
    public async Task<User> RegisterUserAsync(User user)
    {
        var result = await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<List<User>> GetUsersAsync(string? filterOn, string? filterQuery, string? sortBy,
        bool isAscending = true,
        int pageNumber = 1, int pageSize = 100)
    {
        var users = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
        {
            users = filterOn switch
            {
                "email" => users.Where(u => u.Email.Contains(filterQuery)),
                "name" => users.Where(u => u.Name.Contains(filterQuery)),
                _ => users
            };
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            users = isAscending
                ? sortBy switch
                {
                    "email" => users.OrderBy(u => u.Email),
                    "name" => users.OrderBy(u => u.Name),
                    _ => users
                }
                : sortBy switch
                {
                    "email" => users.OrderByDescending(u => u.Email),
                    "name" => users.OrderByDescending(u => u.Name),
                    _ => users
                };
        }

        return await users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }
    
    public async Task<User> LoginUserAsync(User user)
    {
        var result = await context.Users.FirstOrDefaultAsync(u => u.Email == user.Email);

        if (result == null)
        {
            return null!;
        }

        return !PasswordHasher.Verify(user.Password, result.Password) ? null! : result;
    }
    public async Task<User> UpdateUserAsync(User user)
    {
        var result = context.Users.Update(user);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<User> ChangePasswordAsync(User user, string newPassword)
    {
        user.Password = PasswordHasher.Hash(newPassword);
        var result = context.Users.Update(user);
        await context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<User> DeleteUserAsync(Guid id)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return null!;
        }

        user.IsDeleted = true;
        user.CreatedAt = DateTimeOffset.UtcNow;
        var result = context.Users.Update(user);
        await context.SaveChangesAsync();

        return result.Entity;
    }
}