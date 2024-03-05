using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Services;

public class AuditService
{
    private readonly DbContext _context;
    // Additional context or services needed to derive AuditLog fields

    public AuditService(DbContext context) => _context = context;

    public void SaveAuditEntries(IEnumerable<AuditEntry> auditEntries, Guid userId, string method, string reason)
    {
        var auditLogs = auditEntries.Select(entry => new AuditLog
        {
            UserId = userId.ToString(),
            ChangeTime = DateTime.UtcNow, // Assuming current time for simplicity
            Method = method,
            // RecordNumber should be derived or set based on the context of changes.
            Reason = reason
            // Additional fields as necessary based on the audit entry and application context
        }).ToList();

        _context.Set<AuditLog>().AddRange(auditLogs);
        _context.SaveChanges();
    }
}