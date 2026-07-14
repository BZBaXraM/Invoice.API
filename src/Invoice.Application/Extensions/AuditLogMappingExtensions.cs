namespace Invoice.Application.Extensions;

public static class AuditLogMappingExtensions
{
    public static AuditLogResponse ToAuditLogResponse(this AuditLog auditLog) => new()
    {
        Id = auditLog.Id,
        ActorName = auditLog.ActorName,
        ActorEmail = auditLog.ActorEmail,
        EntityType = auditLog.EntityType,
        EntityId = auditLog.EntityId,
        Action = auditLog.Action,
        ChangesJson = auditLog.ChangesJson,
        CreatedAt = auditLog.CreatedAt
    };
}
