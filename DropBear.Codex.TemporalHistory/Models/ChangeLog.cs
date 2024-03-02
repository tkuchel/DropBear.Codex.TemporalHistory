using System.ComponentModel.DataAnnotations;
using DropBear.Codex.TemporalHistory.Enums;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a record of changes made to entities, including the type, time, and reason for each change.
/// </summary>
public class ChangeLog
{
    [Key] public int ChangeLogId { get; init; }

    [Required] [MaxLength(255)] public required string EntityName { get; init; }

    [Required] [MaxLength(255)] public required string EntityKey { get; init; }

    [Required] [MaxLength(50)] public required ChangeTypeEnum ChangeType { get; init; }

    public DateTime ChangeTime { get; init; }

    [MaxLength(255)] public required string UserId { get; init; }

    [MaxLength(1000)] public required string ChangeReason { get; init; }

    public DateTime PeriodStart { get; init; }

    public DateTime PeriodEnd { get; init; }
}
