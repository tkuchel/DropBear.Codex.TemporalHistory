namespace DropBear.Codex.TemporalHistory.Models;

public class AuditLog
{
    public int Id { get; set; } // Primary key
    public string UserId { get; set; } // Who made the change
    public DateTime ChangeTime { get; set; } // When was it made
    public string Method { get; set; } // How was it made
    public int RecordNumber { get; set; } // Link to temporal table record
    public string Reason { get; set; } // Why was the change made

    // Constructor, getters, and setters can be implemented as needed
}
