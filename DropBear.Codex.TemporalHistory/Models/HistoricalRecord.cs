using DropBear.Codex.TemporalHistory.Enums;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a historical record, detailing a specific change to an entity, including the timing and reason for the
///     change.
/// </summary>
public class HistoricalRecord
{
    public int ChangeLogId { get; set; }
    public required string EntityName { get; init; }
    public required string EntityKey { get; init; }
    public required ChangeTypeEnum ChangeType { get; set; }
    public DateTime ChangeTime { get; init; }
    public string? UserId { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}
