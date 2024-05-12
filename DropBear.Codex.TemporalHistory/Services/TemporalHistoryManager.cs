using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Service to manage and query temporal history for entities using Entity Framework Core.
/// </summary>
/// <typeparam name="TContext">The EF database context type this service operates on.</typeparam>
public class TemporalHistoryManager<TContext> : ITemporalHistoryManager<TContext> where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _contextFactory;

    public TemporalHistoryManager(IDbContextFactory<TContext> contextFactory) => _contextFactory = contextFactory;

    /// <summary>
    ///     Retrieves historical records for a given entity type within the specified date range.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="from">The start date of the period.</param>
    /// <param name="to">The end date of the period.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A list of temporal records for the specified entity.</returns>
    public async Task<IEnumerable<TemporalRecord<T>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class =>
        await WithContextAsync(async context =>
        {
            return await context.Set<T>()
                .TemporalAll()
                .Where(e => EF.Property<DateTime>(e, "PeriodStart") >= from &&
                            EF.Property<DateTime>(e, "PeriodEnd") <= to)
                .Select(e => new TemporalRecord<T>
                {
                    Entity = e,
                    ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                    ValidTo = EF.Property<DateTime>(e, "PeriodEnd"),
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);

    /// <summary>
    ///     Retrieves a snapshot of an entity at a specific point in time.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="pointInTime">The specific point in time for the snapshot.</param>
    /// <param name="key">The key value of the entity.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A snapshot of the entity if found; otherwise, null.</returns>
    public async Task<T?> GetEntitySnapshotAt<T>(DateTime pointInTime, object key,
        CancellationToken cancellationToken = default) where T : class =>
        await WithContextAsync(async context =>
        {
            return await context.Set<T>()
                .TemporalAsOf(pointInTime)
                .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id") == key, cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);

    /// <summary>
    ///     Retrieves the latest changes for a given entity type, limited by a maximum number of results.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <param name="maxResults">The maximum number of records to retrieve.</param>
    /// <param name="cancellationToken">A token for cancelling the operation.</param>
    /// <returns>A list of the latest temporal records.</returns>
    public async Task<IEnumerable<TemporalRecord<T>>> GetLatestChanges<T>(int maxResults,
        CancellationToken cancellationToken = default) where T : class =>
        await WithContextAsync(async context =>
        {
            return await context.Set<T>()
                .TemporalAll()
                .OrderByDescending(e => EF.Property<DateTime>(e, "PeriodEnd"))
                .Take(maxResults)
                .Select(e => new TemporalRecord<T>
                {
                    Entity = e,
                    ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                    ValidTo = EF.Property<DateTime>(e, "PeriodEnd"),
                })
                .ToListAsync(cancellationToken).ConfigureAwait(false);
        }).ConfigureAwait(false);

    /// <summary>
    ///     Creates a DbContext instance using the context factory.
    /// </summary>
    /// <returns>An instance of TContext.</returns>
    private async Task<TContext?> CreateDbContextAsync() =>
        await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);

    /// <summary>
    ///     Executes a function that requires a DbContext within a using block ensuring proper disposal.
    /// </summary>
    /// <typeparam name="TResult">The result type of the function.</typeparam>
    /// <param name="operation">The function to execute that requires a DbContext.</param>
    /// <returns>The result of the function.</returns>
    private async Task<TResult> WithContextAsync<TResult>(Func<TContext, Task<TResult>> operation)
    {
        TContext? context = null;
        try
        {
            context = await _contextFactory.CreateDbContextAsync().ConfigureAwait(false);
            return await operation(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (context is not null)
                TemporalHistoryManager<TContext>.LogError(ex, context);
            throw; // Re-throw to preserve stack trace
        }
    }

    private static void LogError(Exception ex, TContext? context)
    {
        // Here you could include specific details about the context, such as the current state of the data model
        var dataState = context?.ChangeTracker?.Entries() ?? [];
        var entityStates = dataState.Select(e => new { Entity = e.Entity.GetType().Name, State = e.State });

        // Log the exception with structured data
        Console.WriteLine($"An error occurred executing database operation. Context state: {entityStates}");
    }
}
