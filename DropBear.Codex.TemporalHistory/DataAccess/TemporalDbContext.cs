using DropBear.Codex.TemporalHistory.Configurations;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DropBear.Codex.TemporalHistory.DataAccess;

public class TemporalDbContext : DbContext
{
    private readonly AuditableConfig _auditableConfig;
    private readonly ILogger<TemporalDbContext> _logger;

    public TemporalDbContext(AuditableConfig auditableConfig, ILogger<TemporalDbContext> logger)
    {
        _auditableConfig = auditableConfig;
        _logger = logger;
    }

    public DbSet<ChangeLog> ChangeLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyTemporalTableConfiguration();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CaptureChangeMetadata();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        CaptureChangeMetadata();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void CaptureChangeMetadata()
    {
        var userId = GetCurrentUserId();
        var reason = ""; // Implement logic to determine reason or pass as a parameter.

        foreach (var entry in ChangeTracker.Entries())
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                var entityType = entry.Entity.GetType();
                var changeLog = new ChangeLog
                {
                    EntityName = entityType.Name,
                    EntityKey =
                        entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue.ToString(),
                    ChangeType = entry.State.ToString(),
                    ChangeTime = DateTime.UtcNow,
                    UserId = userId,
                    ChangeReason = reason, // This needs to be determined based on your application logic.
                    PeriodStart = DateTime.UtcNow, // Adjust as necessary
                    PeriodEnd = DateTime.MaxValue // Adjust as necessary
                };

                ChangeLogs.Add(changeLog);
            }
    }

    /// <summary>
    ///     Updates audit information for auditable entities.
    /// </summary>
    /// <param name="entity">The entity being audited.</param>
    /// <param name="state">The state of the entity.</param>
    private void UpdateAuditInformation(object entity, EntityState state)
    {
        if (entity is IAuditable auditable)
        {
            var mapping =
                _auditableConfig.GetMapping(entity.GetType()) ??
                new AuditableFieldMapping(); // Default or custom mapping
            var now = DateTime.UtcNow;
            var userId = GetCurrentUserId(); // Implement this based on your application's user context

            if (state == EntityState.Added)
            {
                typeof(IAuditable).GetProperty(mapping.CreatedAt)?.SetValue(auditable, now);
                typeof(IAuditable).GetProperty(mapping.CreatedBy)?.SetValue(auditable, userId);
            }

            typeof(IAuditable).GetProperty(mapping.ModifiedAt)?.SetValue(auditable, now);
            typeof(IAuditable).GetProperty(mapping.ModifiedBy)?.SetValue(auditable, userId);
        }
    }

    /// <summary>
    ///     Placeholder for a method to fetch the current user's ID.
    ///     Implementation depends on your application's authentication mechanism.
    /// </summary>
    /// <returns>The current user's ID.</returns>
    private string GetCurrentUserId() => throw new NotImplementedException();

    /// <summary>
    ///     Placeholder for a method to fetch the reason for a change.
    ///     Implementation can vary. You might use a specific interface or annotations to extract reasons from entities.
    /// </summary>
    /// <param name="entity">The entity being modified.</param>
    /// <returns>The reason for the change.</returns>
    private string GetChangeReason(object entity) => throw new NotImplementedException();
}
