namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a single property change in a temporal entity.
/// </summary>
public class PropertyChange
{
    public PropertyChange()
    {
        PropertyName = string.Empty;
        OriginalValue = null;
        CurrentValue = null;
    }

    /// <summary>
    ///     Initializes a new instance of the PropertyChange class.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    /// <param name="originalValue">The original value of the property.</param>
    /// <param name="currentValue">The new value of the property.</param>
    public PropertyChange(string propertyName, object? originalValue, object? currentValue)
    {
        PropertyName = propertyName;
        OriginalValue = originalValue;
        CurrentValue = currentValue;
    }

    /// <summary>
    ///     Gets or sets the name of the property that has changed.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    ///     Gets or sets the original value of the property before the change.
    /// </summary>
    public object? OriginalValue { get; set; }

    /// <summary>
    ///     Gets or sets the current value of the property after the change.
    /// </summary>
    public object? CurrentValue { get; set; }
}
