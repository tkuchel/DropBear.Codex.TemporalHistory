namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
/// Represents a change to a single property of an entity being audited.
/// </summary>
public class PropertyChange
{
    public string PropertyName { get; }
    public object? OriginalValue { get; }
    public object? CurrentValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChange"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property that has changed.</param>
    /// <param name="originalValue">The original value of the property before the change.</param>
    /// <param name="currentValue">The new value of the property after the change.</param>
    public PropertyChange(string propertyName, object? originalValue, object? currentValue)
    {
        PropertyName = propertyName;
        OriginalValue = originalValue;
        CurrentValue = currentValue;
    }
}
