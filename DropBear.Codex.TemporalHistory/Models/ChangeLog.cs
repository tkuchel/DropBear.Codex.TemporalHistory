using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DropBear.Codex.TemporalHistory.Models;

/// <summary>
/// Represents a log of changes made to entities within the system, capturing the nature and timing of each change.
/// </summary>
public class ChangeLog
{
    [Key]
    public int ChangeLogId { get; set; }

    [Required]
    [MaxLength(255)]
    public string EntityName { get; set; }

    [Required]
    [MaxLength(255)]
    public string EntityKey { get; set; }

    [Required]
    [MaxLength(50)]
    public string ChangeType { get; set; } // Consider enum for Add, Update, Delete

    public DateTime ChangeTime { get; set; }

    [MaxLength(255)]
    public string UserId { get; set; }

    [MaxLength(1000)]
    public string ChangeReason { get; set; }

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }
}
