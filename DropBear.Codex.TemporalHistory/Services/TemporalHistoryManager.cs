using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Service to manage and query temporal history for entities using Entity Framework.
/// </summary>
/// <typeparam name="TContext">The EF database context type this service operates on.</typeparam>
public class TemporalHistoryManager<TContext> : ITemporalHistoryManager<TContext> where TContext : DbContext
{
    private readonly TContext _context;

    public TemporalHistoryManager(TContext context) => _context = context;

    public async Task<IEnumerable<TemporalRecord<T>>> GetHistoryAsync<T>(DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalAll()
            .Where(e => EF.Property<DateTime>(e, "PeriodStart") >= from && EF.Property<DateTime>(e, "PeriodEnd") <= to)
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(e, "PeriodEnd")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<T?> GetEntitySnapshotAt<T>(DateTime pointInTime, object key,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalAsOf(pointInTime)
            .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id") == key, cancellationToken).ConfigureAwait(false);

    public async Task<T?> GetEntitySnapshotAt<T>(DateTime snapshotTime, CancellationToken cancellationToken = default)
        where T : class =>
        await _context.Set<T>()
            .TemporalAsOf(snapshotTime)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);


    public async Task<IEnumerable<TemporalRecord<T>>> GetLatestChanges<T>(int maxResults,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalAll()
            .OrderByDescending(e => EF.Property<DateTime>(e, "PeriodEnd"))
            .Take(maxResults)
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(e, "PeriodEnd")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IEnumerable<TemporalRecord<T>>> GetChangesByUser<T>(string userId, DateTime from, DateTime to,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalAll()
            .Where(e => EF.Property<DateTime>(e, "PeriodStart") >= from &&
                        EF.Property<DateTime>(e, "PeriodEnd") <= to && EF.Property<string>(e, "ModifiedBy") == userId)
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(e, "PeriodEnd")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    public async Task<IEnumerable<PropertyChange>> CompareEntityStates<T>(DateTime fromTime, DateTime toTime,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityAtFromTime = await _context.Set<T>()
            .TemporalAsOf(fromTime)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        var entityAtToTime = await _context.Set<T>()
            .TemporalAsOf(toTime)
            .SingleOrDefaultAsync(cancellationToken).ConfigureAwait(false);

        if (entityAtFromTime is null || entityAtToTime is null)
            return new List<PropertyChange>(); // Return an empty list or handle this case as needed

        return TemporalHistoryManager<TContext>.CompareEntities(entityAtFromTime, entityAtToTime);
    }

    public async Task<IEnumerable<TemporalRecord<T>>> GetHistoryForPeriod<T>(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken = default) where T : class =>
        await _context.Set<T>()
            .TemporalFromTo(startDate, endDate)
            .OrderBy(e => EF.Property<DateTime>(e, "PeriodStart"))
            .Select(e => new TemporalRecord<T>
            {
                Entity = e,
                ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(e, "PeriodEnd")
            })
            .ToListAsync(cancellationToken).ConfigureAwait(false);

    private static List<PropertyChange> CompareEntities<T>(T oldEntity, T newEntity) where T : class
    {
        // Assuming you have a way to reflect over the properties of T to find differences
        var properties = typeof(T).GetProperties();

        return (from property in properties
            let oldValue = property.GetValue(oldEntity)
            let newValue = property.GetValue(newEntity)
            where !Equals(oldValue, newValue)
            select new PropertyChange
            {
                PropertyName = property.Name, OriginalValue = oldValue, CurrentValue = newValue
            }).ToList();
    }
}
