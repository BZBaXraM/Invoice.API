namespace Invoice.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid UserId { get; set; }
    public required string ActorName { get; set; }
    public string? ActorEmail { get; set; }
    public required string EntityType { get; set; }
    public Guid EntityId { get; set; }
    public required string Action { get; set; }
    public required string ChangesJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
