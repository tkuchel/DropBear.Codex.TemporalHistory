using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Configurations;

/// <summary>
///     Holds configuration options for auditing a specific entity, including field mappings.
/// </summary>
public class EntityAuditConfig
{
    public bool AuditingEnabled { get; set; } = true;

    public AuditableFieldMapping? FieldMapping { get; set; }
    // Additional configuration options can be added here
}
