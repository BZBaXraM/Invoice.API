using InvoiceManager.API.Data;
using InvoiceManager.API.DTOs;
using InvoiceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.API.Services;

public class UserService(InvoiceDbContext context) : IAsyncUserService
{
    public async Task<User> Register(UserRegisterDto dto)
    {
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> Login(UserLoginDto dto)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        return user;
    }

    public async Task<User> UpdateProfile(int userId, UserUpdateDto dto)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.Name = dto.Name;
        user.Address = dto.Address;
        user.PhoneNumber = dto.PhoneNumber;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync();
        return user;
    }

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