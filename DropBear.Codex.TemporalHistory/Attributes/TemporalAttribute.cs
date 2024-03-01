namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
/// Marks an entity class as requiring support for temporal tables.
/// This attribute should be applied to entity classes that need to be backed
/// by SQL Server's system-versioned temporal tables, enabling automatic
/// tracking of historical data changes.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TemporalAttribute : Attribute
{
    public string HistoryTableName { get; set; }
    public string PeriodStartColumnName { get; set; } = "ValidFrom";
    public string PeriodEndColumnName { get; set; } = "ValidTo";
}

