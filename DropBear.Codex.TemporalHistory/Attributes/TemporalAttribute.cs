namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
///     Marks an entity class as requiring support for temporal tables, enabling historical data change tracking.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TemporalAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the name of the history table.
    /// </summary>
    public string? HistoryTableName { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the name of the column representing the start of the valid period. Defaults to "ValidFrom".
    /// </summary>
    public string PeriodStartColumnName { get; set; } = "ValidFrom";

    /// <summary>
    ///     Gets or sets the name of the column representing the end of the valid period. Defaults to "ValidTo".
    /// </summary>
    public string PeriodEndColumnName { get; set; } = "ValidTo";
}
