namespace DropBear.Codex.TemporalHistory.Models;

public class ChangeLog
{
    public int ChangeLogId { get; set; }
    public string EntityName { get; set; }
    public string EntityKey { get; set; }
    public string ChangeType { get; set; } // Add, Update, Delete
    public DateTime ChangeTime { get; set; }
    public string UserId { get; set; }
    public string ChangeReason { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}


