using Microsoft.Extensions.DependencyInjection;


namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
/// Extension methods for configuring services related to temporal history management.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for temporal history management to the service collection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddTemporalHistory(this IServiceCollection services)
    {
        // Register any specific services, repositories, or utilities needed for temporal history management.
        // For example, if you have a service for handling temporal data operations:
        // services.AddScoped<ITemporalDataService, TemporalDataService>();

        // Register a generic audit service if using IAuditable entities
        // services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
