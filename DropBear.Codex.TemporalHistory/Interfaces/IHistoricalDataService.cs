using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Interfaces;

public interface IHistoricalDataService
{
    Task<List<HistoricalRecord>> GetHistoryForRecordAsync<T>(int id) where T : class, ITemporal;
}