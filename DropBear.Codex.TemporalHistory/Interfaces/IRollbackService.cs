namespace DropBear.Codex.TemporalHistory.Interfaces;

public interface IRollbackService
{
    Task<bool> RollbackRecordAsync<T>(int recordId, DateTime toDateTime) where T : class, ITemporal;
}