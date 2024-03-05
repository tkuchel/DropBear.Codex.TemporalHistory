using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents an audit entry for an entity, capturing the state change and any property changes.
/// </summary>
public class AuditEntry
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditEntry" /> class.
    /// </summary>
    /// <param name="entity">The entity being audited.</param>
    /// <param name="state">The state of the entity.</param>
    /// <param name="entityId">The identifier of the entity.</param>
    /// <param name="lastModifiedBy">The identifier of the user who last modified the entity.</param>
    /// <param name="lastModifiedAt">The date and time when the entity was last modified.</param>
    public AuditEntry(object entity, EntityState state, Guid entityId, Guid lastModifiedBy, DateTime lastModifiedAt)
    {
        EntityType = entity.GetType();
        State = state;
        EntityId = entityId;
        LastModifiedBy = lastModifiedBy;
        LastModifiedAt = lastModifiedAt;
        Changes = new List<PropertyChange>();
    }

    public Type EntityType { get; }
    public EntityState State { get; }
    private List<PropertyChange> Changes { get; }
    public Guid EntityId { get; }
    public Guid LastModifiedBy { get; }
    public DateTime LastModifiedAt { get; }

    /// <summary>
    ///     Adds a change to the list of property changes for this audit entry.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="originalValue">The original value of the property.</param>
    /// <param name="currentValue">The current value of the property.</param>
    public void AddChange(string propertyName, object? originalValue, object? currentValue) =>
        Changes.Add(new PropertyChange(propertyName, originalValue, currentValue));
}
