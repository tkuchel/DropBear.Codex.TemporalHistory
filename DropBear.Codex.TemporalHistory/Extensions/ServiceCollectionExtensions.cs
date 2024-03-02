using DropBear.Codex.TemporalHistory.Configurations;
using DropBear.Codex.TemporalHistory.DataAccess;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds services and configurations required for temporal history management to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureAuditableConfig">An action to configure the AuditableConfig instance.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddTemporalHistory(this IServiceCollection services,
        Action<AuditableConfig>? configureAuditableConfig = null)
    {
        // Assumes ILoggerFactory is configured by the consuming application.
        services.AddSingleton(provider =>
        {
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<AuditableConfig>();

            var auditableConfig = new AuditableConfig(logger);
            configureAuditableConfig?.Invoke(auditableConfig);
            return auditableConfig;
        });

        // Register the DbContext and other services as needed
        services.AddDbContext<TemporalDbContext>(); // Ensure this is configured as per the application's needs
        services.AddScoped<IHistoricalDataService, HistoricalDataService>();
        services.AddScoped<IRollbackService, RollbackService>();

        return services;
    }
}
