namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
///     Maps database field names to properties used for auditing purposes.
/// </summary>
public class AuditableFieldMapping
{
    public string CreatedAt { get; set; } = "CreatedAt";
    public string CreatedBy { get; set; } = "CreatedBy";
    public string ModifiedAt { get; set; } = "ModifiedAt";
    public string ModifiedBy { get; set; } = "ModifiedBy";
}
