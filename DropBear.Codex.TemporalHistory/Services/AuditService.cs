using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

/// <summary>
///     Provides functionality to save audit entries into the database.
/// </summary>
public class AuditService
{
    private readonly AuditContext _auditContext;
    private readonly DbContext _context;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditService" /> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="auditContext">The context containing audit information.</param>
    public AuditService(DbContext context, AuditContext auditContext)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _auditContext = auditContext ?? throw new ArgumentNullException(nameof(auditContext));
    }

    /// <summary>
    ///     Saves a collection of audit entries to the database.
    /// </summary>
    /// <param name="auditEntries">The audit entries to save.</param>
    public void SaveAuditEntries(IEnumerable<AuditEntry> auditEntries)
    {
        var auditLogs = auditEntries.Select(entry => new AuditLog
        {
            UserId = _auditContext.UserId.ToString(),
            ChangeTime = DateTime.UtcNow,
            OperationCode = _auditContext.OperationCode.ToString(),
            Reason = _auditContext.Reason ?? string.Empty,
            RecordNumber = entry.EntityId,
        }).ToList();

        _context.Set<AuditLog>().AddRange(auditLogs);
        _context.SaveChanges();
    }
}