using InvoiceManager.API.Data;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;
using InvoiceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Services.Classes;

/// <summary>
/// The user service.
/// </summary>
/// <param name="context"></param>
public class UserService(InvoiceDbContext context) : IAsyncUserService
{
    /// <summary>
    /// The database context.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<User> Register(UserRegisterDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Address = dto.Address,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Login the user.
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task<User> Login(UserLoginDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return user;
    }

    /// <summary>
    /// Update the user profile.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task<User> UpdateProfile(int userId, UserUpdateDto dto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Address = dto.Address;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = dto.UpdatedAt;

        await context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Change the user password.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="UnauthorizedAccessException"></exception>
    public async Task ChangePassword(int userId, ChangePasswordDto dto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid current password");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Delete the user profile.
    /// </summary>
    /// <param name="userId"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public async Task DeleteProfile(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user is null)
        {
            throw new KeyNotFoundException("User not found");
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync();
    }
}