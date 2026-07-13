namespace Invoice.Application.Services;

public class AuditService(IUnitOfWork uow) : IAuditService
{
    public async Task<ResponseModel<PagedResult<AuditLogResponse>>> GetListAsync(
        Guid ownerUserId,
        int pageNumber,
        int pageSize,
        string? entityType,
        Guid? entityId)
    {
        var (items, totalCount) =
            await uow.AuditRepository.GetPagedAsync(ownerUserId, pageNumber, pageSize, entityType, entityId);

        return ResponseModel.Success(new PagedResult<AuditLogResponse>
        {
            Items = items.Select(a => new AuditLogResponse
            {
                Id = a.Id,
                ActorName = a.ActorName,
                ActorEmail = a.ActorEmail,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action,
                ChangesJson = a.ChangesJson,
                CreatedAt = a.CreatedAt
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        });
    }
}
