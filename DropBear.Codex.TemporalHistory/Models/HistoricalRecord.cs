namespace DropBear.Codex.TemporalHistory.Models;

public class HistoricalRecord
{
    public int ChangeLogId { get; set; }
    public string EntityName { get; set; }
    public string EntityKey { get; set; }
    public string ChangeType { get; set; }
    public DateTime ChangeTime { get; set; }
    public string UserId { get; set; }
    public string ChangeReason { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    // Additional properties as needed to represent the state of the entity at this point in history
}
