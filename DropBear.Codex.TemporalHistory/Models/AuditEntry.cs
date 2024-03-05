using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Models;

public class AuditEntry
{
    public Type EntityType { get; }
    public EntityState State { get; }
    public List<PropertyChange> Changes { get; }
    public Guid EntityId { get; } // New property
    public Guid LastModifiedBy { get; } // New property
    public DateTime LastModifiedAt { get; } // New property

    // Adjusted constructor
    public AuditEntry(object entity, EntityState state, Guid entityId, Guid lastModifiedBy, DateTime lastModifiedAt)
    {
        EntityType = entity.GetType();
        State = state;
        EntityId = entityId;
        LastModifiedBy = lastModifiedBy;
        LastModifiedAt = lastModifiedAt;
        Changes = new List<PropertyChange>();
    }

    // Method to add a property change to the entry
    public void AddChange(string propertyName, object originalValue, object currentValue)
    {
        Changes.Add(new PropertyChange(propertyName, originalValue, currentValue));
    }
}

public class PropertyChange
{
    public string PropertyName { get; }
    public object OriginalValue { get; }
    public object CurrentValue { get; }

    public PropertyChange(string propertyName, object originalValue, object currentValue)
    {
        PropertyName = propertyName;
        OriginalValue = originalValue;
        CurrentValue = currentValue;
    }
}

