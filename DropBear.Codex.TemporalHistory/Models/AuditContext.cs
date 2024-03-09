using DropBear.Codex.TemporalHistory.Enums;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Represents the context for an audit operation, including details about the user, reason, and operation type.
/// </summary>
public class AuditContext
{
    private readonly Guid _userId;

    public Guid UserId
    {
        get => _userId;
        init => _userId = value != Guid.Empty
            ? value
            : throw new ArgumentException("UserId cannot be empty.", nameof(value));
    }

    public string? Reason { get; init; }

    public OperationCode OperationCode { get; init; }
}
