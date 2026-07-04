using Invoice.Application.RepositoryContracts;
using Invoice.Domain.Entities;
using Invoice.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Invoice.Infrastructure.Repositories;

public class UserRepository(InvoiceDbContext context) : IUserRepository
{
    public User AddUser(User user)
    {
        context.Users.Add(user);
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id) =>
        await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername) =>
        await context.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername || u.Username == emailOrUsername);

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken) =>
        await context.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

    public async Task<User?> GetByConfirmationCodeAsync(string code) =>
        await context.Users.FirstOrDefaultAsync(u => u.EmailConfirmationCode == code);

    public async Task<User?> GetByPasswordResetCodeAsync(string code) =>
        await context.Users.FirstOrDefaultAsync(u => u.PasswordResetCode == code);

    public void RemoveUser(User user) => context.Users.Remove(user);
}
