namespace DropBear.Codex.TemporalHistory.Models;

public class AuditableFieldMapping
{
    public string CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
}