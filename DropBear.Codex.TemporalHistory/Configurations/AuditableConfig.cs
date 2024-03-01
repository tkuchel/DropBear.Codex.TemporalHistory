using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Configurations;

public class AuditableConfig
{
    public Dictionary<Type, AuditableFieldMapping> Mappings { get; } = new Dictionary<Type, AuditableFieldMapping>();

    public void Configure<T>(AuditableFieldMapping mapping) where T : IAuditable
    {
        Mappings[typeof(T)] = mapping;
    }

    public AuditableFieldMapping GetMapping(Type entityType)
    {
        Mappings.TryGetValue(entityType, out var mapping);
        return mapping;
    }
}
