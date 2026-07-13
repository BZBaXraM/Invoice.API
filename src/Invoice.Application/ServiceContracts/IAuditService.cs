namespace Invoice.Application.ServiceContracts;

public interface IAuditService
{
    Task<ResponseModel<PagedResult<AuditLogResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? entityType,
        Guid? entityId);
}
