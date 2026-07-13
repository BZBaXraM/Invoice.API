namespace Invoice.Application.RepositoryContracts;

public interface IAuditRepository
{
    Task<(List<AuditLog> Items, int TotalCount)> GetPagedAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? entityType,
        Guid? entityId);
}
