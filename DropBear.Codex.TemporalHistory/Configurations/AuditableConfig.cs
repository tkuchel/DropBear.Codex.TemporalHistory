using DropBear.Codex.TemporalHistory.Delegates;
using DropBear.Codex.TemporalHistory.Interfaces;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.TemporalHistory.Configurations;

/// <summary>
///     Provides configuration for auditable entities, including delegate configurations and field mappings.
/// </summary>
public class AuditableConfig(ILogger<AuditableConfig> logger)
{
    private readonly ILogger<AuditableConfig> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public GetCurrentUserIdDelegate? GetCurrentUserIdFunc { get; set; }
    public GetChangeReasonDelegate? GetChangeReasonFunc { get; set; }

    /// <summary>
    ///     Holds configurations for each auditable entity type, including field mappings.
    /// </summary>
    public Dictionary<Type, EntityAuditConfig> EntityConfigs { get; } = new();

    /// <summary>
    ///     Configures audit settings and field mappings for an auditable entity type.
    /// </summary>
    /// <typeparam name="T">The entity type implementing IAuditable.</typeparam>
    /// <param name="configure">A configuration action for the entity's audit settings.</param>
    public void ConfigureEntity<T>(Action<EntityAuditConfig>? configure) where T : IAuditable
    {
        var config = new EntityAuditConfig();
        configure?.Invoke(config);

        EntityConfigs[typeof(T)] = config;
        _logger.ZLogInformation($"Configured audit settings for {typeof(T).Name}.");
    }

    /// <summary>
    ///     Retrieves audit settings for a specific entity type, if available.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <returns>The entity's audit configuration or null if not found.</returns>
    public EntityAuditConfig? GetEntityConfig<T>() where T : IAuditable
    {
        EntityConfigs.TryGetValue(typeof(T), out var config);
        return config;
    }
}


// Example use.
// services.AddSingleton<AuditableConfig>(provider =>
// {
//     var logger = provider.GetRequiredService<ILogger<AuditableConfig>>();
//     var config = new AuditableConfig(logger);
//
//     config.GetCurrentUserIdFunc = () => "User ID logic";
//     config.GetChangeReasonFunc = (entity) => "Change reason logic";
//
//     // Configure specific entity
//     config.ConfigureEntity<MyAuditableEntity>(entityConfig =>
//     {
//         entityConfig.AuditingEnabled = true;
//         entityConfig.FieldMapping = new AuditableFieldMapping
//         {
//             CreatedAt = "CreatedAtPropertyName",
//             CreatedBy = "CreatedByPropertyName",
//             ModifiedAt = "ModifiedAtPropertyName",
//             ModifiedBy = "ModifiedByPropertyName"
//         };
//     });
//
//     return config;
// });
