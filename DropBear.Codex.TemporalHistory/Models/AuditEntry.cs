using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents an audit entry for an entity, capturing the state change and any property changes.
/// </summary>
public class AuditEntry
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditEntry" /> class for a specified entity.
    /// </summary>
    /// <param name="entityType">The type of the entity being audited.</param>
    /// <param name="state">The state of the entity at the time of audit.</param>
    /// <param name="entityId"> The id for the specific entity </param>
    public AuditEntry(Type entityType, EntityState state, Guid entityId)
    {
        EntityType = entityType;
        State = state;
        Changes = new Collection<PropertyChange>();
        EntityId = entityId;
    }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Type EntityType { get; }

    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public EntityState State { get; }

    // ReSharper disable once CollectionNeverQueried.Local
    private Collection<PropertyChange> Changes { get; }

    public Guid EntityId { get; set; }

    /// <summary>
    ///     Adds a change to the list of property changes for this audit entry.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="originalValue">The original value of the property.</param>
    /// <param name="currentValue">The current value of the property.</param>
    public void AddChange(string propertyName, object? originalValue, object? currentValue) =>
        Changes.Add(new PropertyChange(propertyName, originalValue, currentValue));
}
