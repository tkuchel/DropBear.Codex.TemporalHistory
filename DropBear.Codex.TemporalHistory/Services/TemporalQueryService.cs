using System.Linq.Expressions;
using DropBear.Codex.AppLogger.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

public class TemporalQueryService<T>(DbContext context, IAppLogger<TemporalQueryService<T>> logger)
    where T : class
{
    private readonly DbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    private readonly IAppLogger<TemporalQueryService<T>> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger)); // Initialize logger
    // Logger instance

    /// <summary>
    ///     Synchronously retrieves the historical states for an entity identified by a key within a specified date range.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
    /// <param name="idSelector">An expression to select the entity key.</param>
    /// <param name="entityId">The value of the key for the entity.</param>
    /// <param name="from">The start date of the historical period.</param>
    /// <param name="to">The end date of the historical period.</param>
    /// <returns>
    ///     An enumerable collection of entity
    ///     states within the specified period.
    /// </returns>
    public IEnumerable<T> GetHistoryForKey<TKey>(
        Expression<Func<T, TKey>> idSelector,
        TKey entityId,
        DateTime from,
        DateTime to)
    {
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        // Assuming _context.Set<T>() supports temporal queries directly
        var query = _context.Set<T>()
            .TemporalFromTo(from, to)
            .AsQueryable();

        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(idSelector.Body, Expression.Constant(entityId)),
            idSelector.Parameters);

        var result = query.Where(predicate).ToList();
        return result;
    }


    /// <summary>
    ///     Asynchronously retrieves the historical states for an entity identified by a key within a specified date range.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
    /// <param name="idSelector">An expression to select the entity key.</param>
    /// <param name="entityId">The value of the key for the entity.</param>
    /// <param name="from">The start date of the historical period.</param>
    /// <param name="to">The end date of the historical period.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an enumerable collection of entity
    ///     states within the specified period.
    /// </returns>
    public async Task<IEnumerable<T>> GetHistoryForKeyAsync<TKey>(
        Expression<Func<T, TKey>> idSelector,
        TKey entityId,
        DateTime from,
        DateTime to)
    {
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        // Assuming _context.Set<T>() supports temporal queries directly
        var query = _context.Set<T>()
            .TemporalFromTo(from, to)
            .AsQueryable();

        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(idSelector.Body, Expression.Constant(entityId)),
            idSelector.Parameters);

        var result = await query.Where(predicate).ToListAsync().ConfigureAwait(false);
        return result;
    }

    /// <summary>
    ///     Asynchronously compares two versions of an entity based on timestamps and identifies property changes.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if idSelector does not target a property.</exception>
    public async Task<IEnumerable<PropertyChange>?> CompareEntityVersionsAsync<TKey>( TKey entityId,
        DateTime firstDate, DateTime secondDate)
    {
        if (firstDate >= secondDate)
            throw new ArgumentException("First date must be before the second date.", nameof(firstDate));

        try
        {
            // Adjusted to asynchronous querying using FirstOrDefaultAsync
            var firstState = await _context.Set<T>().TemporalAsOf(firstDate)
                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(entityId)).ConfigureAwait(false);
            var secondState = await _context.Set<T>().TemporalAsOf(secondDate)
                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id")!.Equals(entityId)).ConfigureAwait(false);

            if (firstState is null || secondState is null) return Enumerable.Empty<PropertyChange>();

            // Compare the two states and identify property changes
            // Example placeholder for comparing entity versions and identifying property changes
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "Error comparing entity versions.");
#pragma warning restore CA1848
            throw; // Re-throwing to maintain the method's contract or handle as needed.
        }

        return null;
    }

    /// <summary>
    ///     Asynchronously counts the number of changes for an entity within a specified date range.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the start date is not before the end date.</exception>
    public async Task<int> GetChangeFrequencyAsync<TKey>(TKey entityId,
        DateTime from, DateTime to)
    {
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        try
        {
            // Corrected to asynchronous count using CountAsync
            return await _context.Set<T>()
                .TemporalFromTo(from, to)
                .CountAsync(e => EF.Property<TKey>(e, "Id")!.Equals(entityId)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "Error counting changes for entity.");
#pragma warning restore CA1848
            throw; // Re-throwing to ensure errors are not silently ignored.
        }
    }

    /// <summary>
    ///     Asynchronously identifies entities that match a specified sequence of patterns within a date range.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if patterns is null, empty, or if from is not before to.</exception>
    public async Task<IEnumerable<T>?> GetEntitiesMatchingPatternAsync(IEnumerable<Expression<Func<T, bool>>> patterns,
        DateTime from,
        DateTime to)
    {
        var expressions = patterns as Expression<Func<T, bool>>[] ?? patterns.ToArray();
        if (patterns is null || expressions.Length is 0)
            throw new ArgumentException("Patterns enumerable cannot be null or empty.", nameof(patterns));
        if (from >= to)
            throw new ArgumentException("The start date must be before the end date.", nameof(from));

        try
        {
            var query = _context.Set<T>().TemporalFromTo(from, to).AsQueryable();

            query = expressions.Aggregate(query, (current, pattern) => current.Where(pattern));

            return await query.ToListAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
#pragma warning disable CA1848
            _logger.LogError(ex, "Error identifying entities matching patterns.");
#pragma warning restore CA1848
            throw; // Re-throwing to maintain method contract and error visibility.
        }
    }
}
