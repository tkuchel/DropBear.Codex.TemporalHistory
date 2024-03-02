using DropBear.Codex.TemporalHistory.Models;

namespace DropBear.Codex.TemporalHistory.Interfaces;

/// <summary>
///     Defines the contract for services that retrieve historical data for records.
/// </summary>
public interface IHistoricalDataService
{
    /// <summary>
    ///     Asynchronously gets the history for a record of a specific type.
    /// </summary>
    /// <typeparam name="T">The type of the record, which must implement ITemporal.</typeparam>
    /// <param name="id">The identifier of the record.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of historical records.</returns>
    Task<List<HistoricalRecord>> GetHistoryForRecordAsync<T>(int id) where T : class, ITemporal;
}
