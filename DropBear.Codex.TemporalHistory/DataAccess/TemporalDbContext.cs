using DropBear.Codex.TemporalHistory.Configurations;
using DropBear.Codex.TemporalHistory.Extensions;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace DropBear.Codex.TemporalHistory.DataAccess;

/// <summary>
///     Represents the database context supporting temporal data tracking and audit logging.
/// </summary>
public class TemporalDbContext()
    : DbContext
{
    private readonly AuditableConfig? _auditableConfig;
    private readonly ILogger<TemporalDbContext>? _logger;

    public TemporalDbContext(AuditableConfig auditableConfig, ILogger<TemporalDbContext> logger) : this()
    {
        _auditableConfig = auditableConfig ?? throw new ArgumentNullException(nameof(auditableConfig));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public DbSet<ChangeLog> ChangeLogs { get; init; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyTemporalTableConfiguration();
        modelBuilder.Entity<ChangeLog>()
            .Property(e => e.ChangeType)
            .HasConversion<string>();
        _logger?.ZLogInformation($"Model creating with temporal table configuration applied.");
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateAuditFields(); // New method to update audit fields
        CaptureChangeMetadata();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(); // Invoke the same for async
        CaptureChangeMetadata();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void UpdateAuditFields()
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is IAuditable &&
                                                                 (e.State == EntityState.Added ||
                                                                  e.State == EntityState.Modified)))
            UpdateAuditInformation(entry.Entity, entry.State);
    }


    /// <summary>
    ///     Captures change metadata for entities tracked by the DbContext, creating a ChangeLog entry for each change.
    /// </summary>
    private void CaptureChangeMetadata()
    {
        _logger?.ZLogDebug($"Capturing change metadata.");
        try
        {
            foreach (var entry in ChangeTracker.Entries().Where(e =>
                         e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                var entity = entry.Entity;
                var userId = GetCurrentUserId(); // Retrieves the current user ID using the configured delegate.
                var reason = GetChangeReason(entity); // Correctly passes the entity to the delegate function.

                var entityType = entity.GetType();
                var entityKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue
                    ?.ToString();
                if (entityKey is null) continue;
                var changeLog = new ChangeLog
                {
                    EntityName = entityType.Name,
                    EntityKey = entityKey,
                    ChangeType = entry.State.ToChangeTypeEnum(),
                    ChangeTime = DateTime.UtcNow,
                    UserId = userId,
                    ChangeReason = reason,
                    PeriodStart = DateTime.UtcNow,
                    PeriodEnd = DateTime.MaxValue
                };

                ChangeLogs.Add(changeLog);
            }
        }
        catch (Exception ex)
        {
            _logger?.ZLogError($"Error capturing change metadata: {ex}");
            throw; // Ensures that the exception is handled or logged appropriately.
        }
    }


    /// <summary>
    ///     Updates audit information for auditable entities based on their state.
    /// </summary>
    /// <param name="entity">The entity being audited.</param>
    /// <param name="state">The state of the entity.</param>
    private void UpdateAuditInformation(object entity, EntityState state)
    {
        // Assuming entity is IAuditable and checking global auditing flag.
        if (entity is not IAuditable auditable) return;

        var entityType = entity.GetType();
        var entityConfig = _auditableConfig?.EntityConfigs.GetValueOrDefault(entityType);

        // Check if auditing is enabled both globally and for the specific entity.
        if ((entityConfig?.AuditingEnabled) is not true) return;

        var now = DateTime.UtcNow;
        var userId = GetCurrentUserId();
        
        try
        {
            var mapping = entityConfig.FieldMapping ?? new AuditableFieldMapping();

            SetProperty(auditable, mapping.CreatedAt, now, state is EntityState.Added);
            SetProperty(auditable, mapping.CreatedBy, userId, state is EntityState.Added);
            SetProperty(auditable, mapping.ModifiedAt, now, state is not EntityState.Deleted);
            SetProperty(auditable, mapping.ModifiedBy, userId, state is not EntityState.Deleted);
        }
        catch (Exception ex)
        {
            _logger?.ZLogError($"Error updating audit information for {entityType.Name}: {ex}");
            throw;
        }
    }

    /// <summary>
    ///     Sets the property value if the property exists and meets the condition.
    /// </summary>
    /// <param name="auditable">The auditable entity.</param>
    /// <param name="propertyName">The name of the property to set.</param>
    /// <param name="value">The value to set.</param>
    /// <param name="condition">The condition under which to set the value.</param>
    private static void SetProperty(IAuditable auditable, string propertyName, object value, bool condition)
    {
        if (!condition) return;
        var propertyInfo = auditable.GetType().GetProperty(propertyName);
        propertyInfo?.SetValue(auditable, value);
    }

    private string GetCurrentUserId()
    {
        if (_auditableConfig?.GetCurrentUserIdFunc is not null) return _auditableConfig.GetCurrentUserIdFunc();
        throw new InvalidOperationException("GetCurrentUserIdFunc is not configured.");
    }

    private string GetChangeReason(object entity)
    {
        if (_auditableConfig?.GetChangeReasonFunc is not null) return _auditableConfig.GetChangeReasonFunc(entity);
        throw new InvalidOperationException("GetChangeReasonFunc is not configured.");
    }
}
