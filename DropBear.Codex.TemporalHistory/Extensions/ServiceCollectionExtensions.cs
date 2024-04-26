using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extensions methods for IServiceCollection to add temporal history services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Adds the necessary services and configurations for supporting temporal history.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The modified IServiceCollection.</returns>
    public static IServiceCollection AddTemporalHistory(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped(typeof(ITemporalHistoryManager<>), typeof(TemporalHistoryManager<>));
        return services;
    }
}
