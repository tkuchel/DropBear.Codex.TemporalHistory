using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extension methods for IServiceCollection to add temporal history services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the necessary services and configurations for supporting temporal history,
    ///     ensuring that memory cache is only added if not already registered.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The modified IServiceCollection.</returns>
    public static IServiceCollection AddTemporalHistory(this IServiceCollection services)
    {
        // Check if an instance of IMemoryCache is already registered
        if (services.All(sd => sd.ServiceType != typeof(IMemoryCache))) services.AddMemoryCache();

        services.AddScoped(typeof(ITemporalHistoryManager<>), typeof(TemporalHistoryManager<>));
        return services;
    }
}
