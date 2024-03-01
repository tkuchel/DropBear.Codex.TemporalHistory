using DropBear.Codex.TemporalHistory.Configurations;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.DataAccess;

/// <summary>
///     Represents the base DbContext with support for temporal table change tracking.
/// </summary>
public class TemporalDbContext : DbContext
{
    private readonly AuditableConfig _auditableConfig;

    public TemporalDbContext(AuditableConfig auditableConfig) => _auditableConfig = auditableConfig;

    /// <summary>
    ///     Gets or sets the DbSet for ChangeLogs.
    /// </summary>
    public DbSet<ChangeLog> ChangeLogs { get; set; }


    /// <summary>
    ///     Configures the model that was discovered by convention from the entity types
    ///     exposed in DbSet properties on your derived context. The resulting model may be cached
    ///     and re-used for subsequent instances of your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyTemporalTableConfiguration();
    }

    /// <summary>
    ///     Saves all changes made in this context to the database with change metadata capture.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether changes should be accepted upon successful save.</param>
    /// <returns>The number of state entries written to the database.</returns>
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        CaptureChangeMetadata();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    ///     Asynchronously saves all changes made in this context to the database with change metadata capture.
    /// </summary>
    /// <param name="acceptAllChangesOnSuccess">Indicates whether changes should be accepted upon successful save.</param>
    /// <param name="cancellationToken">A CancellationToken to observe while waiting for the task to complete.</param>
    /// <returns>
    ///     A task that represents the asynchronous save operation. The task result contains the number of state entries
    ///     written to the database.
    /// </returns>
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        CaptureChangeMetadata();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    ///     Captures change metadata for entities being tracked by the DbContext.
    /// </summary>
    private void CaptureChangeMetadata()
    {
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
                    ChangeTime = DateTime.UtcNow // Default, adjust based on IAuditable if available
                };

                // Handle IAuditable fields
                // Check if the entity is auditable and has custom config
                if (typeof(IAuditable).IsAssignableFrom(entityType))
                {
                    var mapping = _auditableConfig.GetMapping(entityType);

                    if (mapping != null)
                    {
                        // Use mapping to get field values
                        var createdAt = entry.Property(mapping.CreatedAt).CurrentValue.ToString();
                        var createdBy = entry.Property(mapping.CreatedBy).CurrentValue.ToString();
                        // Handle ModifiedAt and ModifiedBy similarly
                    }
                }

                // Handle ITemporal interface
                if (entry.Entity is ITemporal temporalEntity)
                {
                    changeLog.PeriodStart = temporalEntity.ValidFrom;
                    changeLog.PeriodEnd = temporalEntity.ValidTo;
                }

                ChangeLogs.Add(changeLog);
            }
    }

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


    // Placeholder for a method to fetch the current user's ID
    private string GetCurrentUserId() => throw
        // Implementation depends on your application's authentication mechanism
        new NotImplementedException();

    // Placeholder for a method to fetch the reason for a change
    private string GetChangeReason(object entity) => throw
        // Implementation can vary. You might use a specific interface or annotations to extract reasons from entities
        new NotImplementedException();
}
