using System.Linq.Expressions;
using DropBear.Codex.TemporalHistory.Bases;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides services for querying the historical states of temporal entities.
/// </summary>
/// <typeparam name="T">The type of temporal entity.</typeparam>
public class TemporalQueryService<T> where T : TemporalEntityBase
{
    private readonly DbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TemporalQueryService{T}" />.
    /// </summary>
    /// <param name="context">The database context.</param>
    public TemporalQueryService(DbContext context) =>
        _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    ///     Retrieves the historical states for an entity identified by a key within a specified date range.
    /// </summary>
    /// <typeparam name="TKey">The type of the key used to identify the entity.</typeparam>
    /// <param name="idSelector">An expression to select the entity key.</param>
    /// <param name="entityId">The value of the key for the entity.</param>
    /// <param name="from">The start date of the historical period.</param>
    /// <param name="to">The end date of the historical period.</param>
    /// <returns>An enumerable collection of entity states within the specified period.</returns>
    public IEnumerable<T> GetHistoryForKey<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime from,
        DateTime to)
    {
        // Assuming _context.Set<T>() supports temporal queries directly
        var temporalQuery = _context.Set<T>()
            .TemporalFromTo(from, to)
            .AsQueryable();

        // Apply the ID selector expression to filter the results
        var predicate = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(idSelector.Body, Expression.Constant(entityId)),
            idSelector.Parameters.First());

        return temporalQuery.Where(predicate).ToList();
    }

    /// <summary>
    ///     Compares two versions of an entity based on timestamps and identifies property changes.
    /// </summary>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The entity's identifier value.</param>
    /// <param name="firstDate">The timestamp for the first version to compare.</param>
    /// <param name="secondDate">The timestamp for the second version to compare.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>A collection of PropertyChange detailing differences between the two versions.</returns>
    public IEnumerable<PropertyChange> CompareEntityVersions<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId,
        DateTime firstDate, DateTime secondDate)
    {
        if (firstDate >= secondDate)
            throw new ArgumentException("First date must be less than second date.", nameof(firstDate));

        // Correct approach to fetch entity states at specific points in time
        var firstState = _context.Set<T>().TemporalAsOf(firstDate)
            .SingleOrDefault(e => idSelector.Compile()(e).Equals(entityId));
        var secondState = _context.Set<T>().TemporalAsOf(secondDate)
            .SingleOrDefault(e => idSelector.Compile()(e).Equals(entityId));

        if (firstState == null || secondState == null) return Enumerable.Empty<PropertyChange>();

        // Proceed with comparison logic as before
        var differences = typeof(T).GetProperties()
            .Where(prop => prop.CanRead)
            .Select(prop => new PropertyChange(prop.Name, prop.GetValue(firstState), prop.GetValue(secondState)))
            .Where(change => !Equals(change.OriginalValue, change.CurrentValue))
            .ToList();

        return differences;
    }

    /// <summary>
    ///     Counts the number of changes for an entity within a specified date range.
    /// </summary>
    /// <param name="idSelector">An expression to select the entity's identifier.</param>
    /// <param name="entityId">The entity's identifier value.</param>
    /// <param name="from">The start date of the range to count changes.</param>
    /// <param name="to">The end date of the range to count changes.</param>
    /// <exception cref="ArgumentException"></exception>
    /// <returns>The number of changes made to the entity within the date range.</returns>
    public int GetChangeFrequency<TKey>(Expression<Func<T, TKey>> idSelector, TKey entityId, DateTime from, DateTime to)
    {
        if (from >= to) throw new ArgumentException("The start date must be before the end date.", nameof(from));

        // Correct approach to count historical states within a specified date range
        var historyCount = _context.Set<T>()
            .TemporalFromTo(from, to)
            .Count(e => idSelector.Compile()(e).Equals(entityId));

        return historyCount;
    }

    /// <summary>
    ///     Identifies entities that match a specified sequence of patterns within a date range.
    /// </summary>
    /// <param name="patterns">An enumerable of expressions defining the patterns to match against entity states.</param>
    /// <param name="from">The start date for matching patterns.</param>
    /// <param name="to">The end date for matching patterns.</param>
    /// <exception cref="ArgumentException">Thrown if patterns is null or empty, or if from is not before to.</exception>
    /// <returns>An enumerable of entities matching the specified patterns within the date range.</returns>
    public IEnumerable<T> GetEntitiesMatchingPattern(IEnumerable<Expression<Func<T, bool>>> patterns, DateTime from,
        DateTime to)
    {
        if (patterns == null || !patterns.Any())
            throw new ArgumentException("Patterns enumerable cannot be null or empty.", nameof(patterns));
        if (from >= to)
            throw new ArgumentException("The start date must be before the end date.", nameof(from));

        var temporalQuery = _context.Set<T>().TemporalFromTo(from, to);

        // This simplistic approach fetches entities matching any of the patterns within the time frame
        // A more complex logic may be required depending on pattern matching requirements
        var matchingEntities = patterns
            .SelectMany(pattern => temporalQuery.Where(pattern).AsEnumerable())
            .Distinct()
            .ToList();

        return matchingEntities;
    }
}
