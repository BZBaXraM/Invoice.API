using Invoice.Application.Helpers;

namespace Invoice.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
        await context.Database.MigrateAsync();

        await SeedAdminAsync(context, scope.ServiceProvider.GetRequiredService<IConfiguration>());
    }

    private static async Task SeedAdminAsync(InvoiceDbContext context, IConfiguration configuration)
    {
        if (await context.Users.AnyAsync(u => u.Role == UserRole.Admin))
        {
            return;
        }

        var email = configuration["AdminSeed:Email"] ?? "admin@invoice.com";
        var password = configuration["AdminSeed:Password"] ?? "admin202210BK";

        context.Users.Add(new User
        {
            FirstName = "Admin",
            LastName = "Admin",
            Username = "admin",
            Email = email,
            Password = PasswordHelper.Hash(password),
            IsEmailConfirmed = true,
            IsActive = true,
            Role = UserRole.Admin,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        await context.SaveChangesAsync();
    }
}
