namespace Invoice.Application.DTOs;

/// <summary>
/// Public representation of an audit log entry (who changed what, and when).
/// </summary>
public class AuditLogResponse
{
    /// <summary>The entry's unique identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>Who made the change ("system" for scheduler-driven changes).</summary>
    public required string ActorName { get; set; }

    /// <summary>The actor's email, when known.</summary>
    public string? ActorEmail { get; set; }

    /// <summary>The changed entity's type name, e.g. Invoice or Customer.</summary>
    public required string EntityType { get; set; }

    /// <summary>The changed entity's id.</summary>
    public Guid EntityId { get; set; }

    /// <summary>Created, Updated or Deleted.</summary>
    public required string Action { get; set; }

    /// <summary>Per-property old/new values as a JSON object string.</summary>
    public required string ChangesJson { get; set; }

    /// <summary>When the change happened.</summary>
    public DateTimeOffset CreatedAt { get; set; }
}
