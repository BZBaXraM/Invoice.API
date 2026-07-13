namespace Invoice.Infrastructure.Repositories;

public class AuditRepository(InvoiceDbContext context) : IAuditRepository
{
    public async Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? entityType,
        Guid? entityId)
    {
        var query = context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        query = query.OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
