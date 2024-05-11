using System;

namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
/// Attribute to designate a class as a temporal entity.
/// This attribute is used to mark classes that should be tracked
/// for historical changes over time. It allows specifying custom settings
/// like the history table name.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TemporalEntityAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the history table for the temporal entity.
    /// If not set, a default naming convention will be used.
    /// </summary>
    public string? HistoryTableName { get; set; }

    /// <summary>
    /// Initializes a new instance of the TemporalEntityAttribute class.
    /// </summary>
    public TemporalEntityAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the TemporalEntityAttribute class with a specific history table name.
    /// </summary>
    /// <param name="historyTableName">The name of the history table to be used for this entity.</param>
    public TemporalEntityAttribute(string historyTableName)
    {
        HistoryTableName = historyTableName;
    }
}
