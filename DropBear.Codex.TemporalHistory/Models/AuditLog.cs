using DropBear.Codex.TemporalHistory.Enums;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents a log of an audit operation, detailing the change made to an entity.
/// </summary>
public class AuditLog
{
    public int Id { get; init; }

    /// <summary>
    ///     The identifier of the user who made the change.
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    ///     The timestamp when the change was made.
    /// </summary>
    public DateTime ChangeTime { get; init; }

    /// <summary>
    ///     The operation code representing how the change was made.
    /// </summary>
    public OperationCode OperationCode { get; init; }

    /// <summary>
    ///     A unique identifier linking this log to a record in a temporal table.
    /// </summary>
    public Guid RecordNumber { get; init; }

    /// <summary>
    ///     The reason why the change was made.
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
