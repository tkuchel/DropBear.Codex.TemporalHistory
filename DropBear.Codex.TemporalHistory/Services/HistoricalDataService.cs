using System.Globalization;
using DropBear.Codex.TemporalHistory.DataAccess;
using DropBear.Codex.TemporalHistory.Enums;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to retrieve historical data for entities supporting temporal features.
/// </summary>
public class HistoricalDataService(TemporalDbContext context) : IHistoricalDataService
{
    private readonly TemporalDbContext _context = context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    ///     Asynchronously retrieves historical records for a specified entity and ID.
    /// </summary>
    /// <typeparam name="T">The entity type implementing ITemporal.</typeparam>
    /// <param name="id">The entity ID to retrieve history for.</param>
    /// <returns>A list of historical records.</returns>
    public async Task<List<HistoricalRecord>> GetHistoryForRecordAsync<T>(int id) where T : class, ITemporal
    {
        var changeLogs = await GetChangeLogsAsync<T>(id).ConfigureAwait(false);
        var historicalStates = await GetTemporalDataAsync<T>(id).ConfigureAwait(false);
        return CombineHistories(changeLogs, historicalStates);
    }

    private async Task<List<HistoricalRecord>> GetChangeLogsAsync<T>(int id) where T : class, ITemporal =>
        await _context.ChangeLogs
            .Where(c => c.EntityName == typeof(T).Name && c.EntityKey == id.ToString(CultureInfo.InvariantCulture))
            .Select(c => new HistoricalRecord
            {
                ChangeLogId = c.ChangeLogId,
                EntityName = c.EntityName,
                EntityKey = c.EntityKey,
                ChangeType = c.ChangeType,
                ChangeTime = c.ChangeTime,
                UserId = c.UserId,
                ChangeReason = c.ChangeReason,
                PeriodStart = c.PeriodStart,
                PeriodEnd = c.PeriodEnd
            })
            .ToListAsync().ConfigureAwait(false);

    private async Task<List<HistoricalRecord>> GetTemporalDataAsync<T>(int id) where T : class, ITemporal =>
        await _context.Set<T>()
            .TemporalAll()
            .Where(e => EF.Property<int>(e, "Id") == id)
            .Select(e => new HistoricalRecord
            {
                EntityName = typeof(T).Name,
                EntityKey = id.ToString(CultureInfo.InvariantCulture),
                ChangeType = ChangeTypeEnum.NotAvailable,
                PeriodStart = e.ValidFrom,
                PeriodEnd = e.ValidTo
            })
            .ToListAsync().ConfigureAwait(false);

    private static List<HistoricalRecord> CombineHistories(IEnumerable<HistoricalRecord> changeLogs,
        IEnumerable<HistoricalRecord> historicalStates)
    {
        var combinedList = changeLogs.Concat(historicalStates)
            .OrderBy(h => h.PeriodStart)
            .ThenBy(h => h.ChangeTime)
            .ToList();
        return RemoveDuplicates(combinedList);
    }

    private static List<HistoricalRecord> RemoveDuplicates(IEnumerable<HistoricalRecord> combinedList) =>
        combinedList.GroupBy(r => new { r.EntityName, r.EntityKey, r.PeriodStart, r.PeriodEnd })
            .Select(g => g.First())
            .OrderBy(r => r.PeriodStart)
            .ThenBy(r => r.ChangeTime)
            .ToList();
}
