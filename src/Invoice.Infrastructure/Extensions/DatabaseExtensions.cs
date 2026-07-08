namespace Invoice.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<InvoiceDbContext>();
        await context.Database.MigrateAsync();
    }
}