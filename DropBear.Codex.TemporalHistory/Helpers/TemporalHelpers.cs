using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Helpers;

/// <summary>
///     Provides utility functions for working with temporal data in Entity Framework Core.
/// </summary>
public static class TemporalHelpers
{
    /// <summary>
    ///     Asynchronously fetches change timestamps for a specific entity identified by its ID.
    /// </summary>
    /// <typeparam name="T">The type of the temporal entity.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityId">The unique identifier of the entity.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a list of DateTime objects
    ///     representing the change timestamps.
    /// </returns>
    public static async Task<List<DateTime>> FetchChangeTimestampsAsync<T>(DbContext context, Guid entityId)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(context);

        var timestamps = await context.Set<T>()
            .TemporalAll()
            .Where(e => EF.Property<Guid>(e, "Id") == entityId)
            .Select(e => EF.Property<DateTime>(e, "PeriodStart")) // Assuming PeriodStart is available and correct
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync().ConfigureAwait(false);

        return timestamps;
    }

    /// <summary>
    ///     Asynchronously performs a rollback operation on an entity to its state at a specified timestamp.
    /// </summary>
    /// <typeparam name="T">The type of the temporal entity.</typeparam>
    /// <param name="context">The DbContext instance.</param>
    /// <param name="entityId">The unique identifier of the entity to rollback.</param>
    /// <param name="rollbackDate">The timestamp to rollback the entity to.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task RollbackToAsync<T>(DbContext context, Guid entityId, DateTime rollbackDate) where T : class
    {
        ArgumentNullException.ThrowIfNull(context);

        var entity = await context.Set<T>()
            .TemporalAsOf(rollbackDate)
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == entityId).ConfigureAwait(false);

        if (entity is null) return; // Entity not found for the specified rollback date

        // var dbSet = context.Set<T>();
        context.Entry(entity).State = EntityState.Modified; // Marking the entire entity as modified

        await context.SaveChangesAsync().ConfigureAwait(false); // Persist changes to the database
    }
}
