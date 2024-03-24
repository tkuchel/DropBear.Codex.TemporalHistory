using DropBear.Codex.AppLogger.Extensions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTemporalHistory(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddScoped(typeof(ITemporalHistoryService<>), typeof(TemporalHistoryService<>));
        services.AddAppLogger();

        return services;
    }
}
