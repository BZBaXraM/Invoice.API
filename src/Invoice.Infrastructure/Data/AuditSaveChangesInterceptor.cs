using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Invoice.Infrastructure.Data;

/// <summary>
/// Writes an AuditLog row for every create/update/delete of business entities, in the same
/// transaction as the change itself. One choke point covers both HTTP requests and the
/// background scheduler; the actor falls back to "system" when there is no authenticated user.
/// </summary>
public class AuditSaveChangesInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions ChangesJsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Entities that get audited. User is deliberately excluded (it carries credentials).</summary>
    private static readonly Type[] AuditedTypes =
    [
        typeof(Customer),
        typeof(Domain.Entities.Invoice),
        typeof(InvoiceRow),
        typeof(Payment),
        typeof(RecurringInvoice),
        typeof(RecurringInvoiceRow),
        typeof(CompanyProfile)
    ];

    private static readonly string[] SkippedProperties = ["UpdatedAt"];

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is InvoiceDbContext context)
        {
            await AppendAuditLogsAsync(context, cancellationToken);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task AppendAuditLogsAsync(InvoiceDbContext context, CancellationToken cancellationToken)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                        && AuditedTypes.Contains(e.Entity.GetType()))
            .ToList();

        if (entries.Count == 0)
        {
            return;
        }

        var (actorName, actorEmail) = await ResolveActorAsync(context, cancellationToken);
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            var ownerUserId = ResolveOwnerUserId(context, entry);
            if (ownerUserId is null)
            {
                continue;
            }

            var changes = BuildChanges(entry);
            if (entry.State == EntityState.Modified && changes.Count == 0)
            {
                continue;
            }

            context.AuditLogs.Add(new AuditLog
            {
                UserId = ownerUserId.Value,
                ActorName = actorName,
                ActorEmail = actorEmail,
                EntityType = entry.Entity.GetType().Name,
                EntityId = ((BaseEntity)entry.Entity).Id,
                Action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Deleted => "Deleted",
                    _ => "Updated"
                },
                ChangesJson = JsonSerializer.Serialize(changes, ChangesJsonOptions),
                CreatedAt = now
            });
        }
    }

    private async Task<(string Name, string? Email)> ResolveActorAsync(InvoiceDbContext context, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (userId is null)
        {
            return ("system", null);
        }

        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);
        return user is null
            ? ("system", null)
            : ($"{user.FirstName} {user.LastName}", user.Email);
    }

    private static Guid? ResolveOwnerUserId(InvoiceDbContext context, EntityEntry entry) => entry.Entity switch
    {
        Customer c => c.UserId,
        Domain.Entities.Invoice i => i.UserId,
        Payment p => p.UserId,
        RecurringInvoice r => r.UserId,
        CompanyProfile p => p.UserId,
        InvoiceRow row => FindParentOwner(context.ChangeTracker.Entries<Domain.Entities.Invoice>(), row.InvoiceId),
        RecurringInvoiceRow row => FindParentOwner(context.ChangeTracker.Entries<RecurringInvoice>(), row.RecurringInvoiceId),
        _ => null
    };

    private static Guid? FindParentOwner<TParent>(IEnumerable<EntityEntry<TParent>> parents, Guid parentId)
        where TParent : BaseEntity
    {
        var parent = parents.FirstOrDefault(p => p.Entity.Id == parentId)?.Entity;
        return parent switch
        {
            Domain.Entities.Invoice i => i.UserId,
            RecurringInvoice r => r.UserId,
            _ => null
        };
    }

    private static Dictionary<string, Dictionary<string, object?>> BuildChanges(EntityEntry entry)
    {
        var changes = new Dictionary<string, Dictionary<string, object?>>();

        foreach (var property in entry.Properties)
        {
            var name = property.Metadata.Name;
            if (SkippedProperties.Contains(name))
            {
                continue;
            }

            switch (entry.State)
            {
                case EntityState.Added when property.CurrentValue is not null:
                    changes[name] = new Dictionary<string, object?> { ["new"] = Render(property.CurrentValue) };
                    break;
                case EntityState.Deleted when property.OriginalValue is not null:
                    changes[name] = new Dictionary<string, object?> { ["old"] = Render(property.OriginalValue) };
                    break;
                case EntityState.Modified when property.IsModified
                                               && !Equals(property.OriginalValue, property.CurrentValue):
                    changes[name] = new Dictionary<string, object?>
                    {
                        ["old"] = Render(property.OriginalValue),
                        ["new"] = Render(property.CurrentValue)
                    };
                    break;
            }
        }

        return changes;
    }

    private static object? Render(object? value) =>
        value is byte[] bytes ? $"(binary, {bytes.Length} bytes)" : value;
}
