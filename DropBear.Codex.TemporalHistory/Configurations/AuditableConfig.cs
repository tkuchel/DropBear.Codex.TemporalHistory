using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.Extensions.Logging;
using ZLogger; // Ensure ZLogger or ILogger is referenced properly

namespace DropBear.Codex.TemporalHistory.Configurations;

/// <summary>
/// Manages configuration for auditable entities, linking them to their specific audit field mappings.
/// </summary>
public class AuditableConfig
{
    private readonly ILogger<AuditableConfig> _logger;

    public AuditableConfig(ILogger<AuditableConfig> logger)
    {
        _logger = logger;
    }

    public Dictionary<Type, AuditableFieldMapping> Mappings { get; } = new Dictionary<Type, AuditableFieldMapping>();

    /// <summary>
    /// Configures a mapping for an auditable entity type.
    /// </summary>
    /// <param name="mapping">The auditable field mapping to associate with the entity type.</param>
    /// <typeparam name="T">The entity type implementing IAuditable.</typeparam>
    public void Configure<T>(AuditableFieldMapping mapping) where T : IAuditable
    {
        ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
        Mappings[typeof(T)] = mapping;
        _logger.ZLogInformation($"Configuring audit for {typeof(T).Name}");
    }

    /// <summary>
    /// Retrieves the audit field mapping for a specific entity type.
    /// </summary>
    /// <param name="entityType">The entity type to retrieve the mapping for.</param>
    /// <returns>The corresponding auditable field mapping, if available.</returns>
    public AuditableFieldMapping GetMapping(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
        Mappings.TryGetValue(entityType, out var mapping);
        return mapping;
    }
}
