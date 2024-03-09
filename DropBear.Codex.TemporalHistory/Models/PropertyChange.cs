namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a change to a single property of an entity being audited, capturing the property's name and its values
///     before and after the change.
/// </summary>
public class PropertyChange
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PropertyChange" /> class.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    /// <param name="originalValue">
    ///     The original value of the property before the change. Can be null for properties that were
    ///     not set.
    /// </param>
    /// <param name="currentValue">The new value of the property after the change. Can be null for properties that are cleared.</param>
    public PropertyChange(string propertyName, object? originalValue, object? currentValue)
    {
        PropertyName = propertyName;
        OriginalValue = originalValue;
        CurrentValue = currentValue;
    }

    public string PropertyName { get; }
    public object? OriginalValue { get; }
    public object? CurrentValue { get; }

    public override bool Equals(object? obj) =>
        // Delegate to the type-safe Equals method for the actual comparison
        Equals(obj as PropertyChange);

    private bool Equals(PropertyChange? other)
    {
        // Check for null and compare run-time types.
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return PropertyName == other.PropertyName &&
               Equals(OriginalValue, other.OriginalValue) &&
               Equals(CurrentValue, other.CurrentValue);
    }

    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;
            hash = hash * 23 + StringComparer.OrdinalIgnoreCase.GetHashCode(PropertyName);
            hash = hash * 23 + (OriginalValue?.GetHashCode() ?? 0);
            hash = hash * 23 + (CurrentValue?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
