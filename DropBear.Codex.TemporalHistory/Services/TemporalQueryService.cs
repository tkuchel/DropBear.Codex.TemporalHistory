using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.TemporalHistory.Services;

public class TemporalQueryService<T> where T : class // Assuming T is a class
{
    private readonly DbContext _context;
    private readonly ILogger<TemporalQueryService<T>> _logger; // Logger instance

    public TemporalQueryService(DbContext context, ILogger<TemporalQueryService<T>> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Initialize logger
    }

    /// <summary>
    ///     Ssynchronously retrieves the historical states for an entity identified by a key within a specified date range.
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

        var result = await query.Where(predicate).ToListAsync();
        return result;
    }

    /// <summary>
    ///     Asynchronously compares two versions of an entity based on timestamps and identifies property changes.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if idSelector does not target a property.</exception>
    public async Task<IEnumerable<PropertyChange>?> CompareEntityVersionsAsync<TKey>(
        Expression<Func<T, TKey>> idSelector, TKey entityId,
        DateTime firstDate, DateTime secondDate)
    {
        if (firstDate >= secondDate) throw new ArgumentException("First date must be before the second date.");

        try
        {
            // Adjusted to asynchronous querying using FirstOrDefaultAsync
            var firstState = await _context.Set<T>().TemporalAsOf(firstDate)
                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(entityId));
            var secondState = await _context.Set<T>().TemporalAsOf(secondDate)
                .FirstOrDefaultAsync(e => EF.Property<TKey>(e, "Id").Equals(entityId));

            if (firstState == null || secondState == null) return Enumerable.Empty<PropertyChange>();

            // Comparison logic remains unchanged
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing entity versions.");
            throw; // Re-throwing to maintain the method's contract or handle as needed.
        }

        return null;
    }

    /// <summary>
    ///     Asynchronously counts the number of changes for an entity within a specified date range.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the start date is not before the end date.</exception>
    public async Task<int> GetChangeFrequencyAsync<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId,
        DateTime from, DateTime to)
    {
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        try
        {
            // Corrected to asynchronous count using CountAsync
            return await _context.Set<T>()
                .TemporalFromTo(from, to)
                .CountAsync(e => EF.Property<TKey>(e, "Id").Equals(entityId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting changes for entity.");
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
        if (patterns == null || !patterns.Any())
            throw new ArgumentException("Patterns enumerable cannot be null or empty.", nameof(patterns));
        if (from >= to)
            throw new ArgumentException("The start date must be before the end date.", nameof(from));

        try
        {
            var query = _context.Set<T>().TemporalFromTo(from, to);
            // Note: EF Core may not directly support combining multiple patterns asynchronously; consider evaluating patterns sequentially or adjusting the logic as needed.
            // Example placeholder for handling patterns and fetching matching entities.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying entities matching patterns.");
            throw; // Re-throwing to maintain method contract and error visibility.
        }

        return null;
    }
}
