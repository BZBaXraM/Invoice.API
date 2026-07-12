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

    public async Task<(List<User> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchFilter)
    {
        var query = context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchFilter))
        {
            var pattern = $"%{searchFilter}%";
            query = query.Where(u =>
                EF.Functions.ILike(u.Email, pattern) ||
                EF.Functions.ILike(u.Username, pattern) ||
                EF.Functions.ILike(u.FirstName, pattern) ||
                EF.Functions.ILike(u.LastName, pattern));
        }

        query = query.OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public void RemoveUser(User user) => context.Users.Remove(user);
}
