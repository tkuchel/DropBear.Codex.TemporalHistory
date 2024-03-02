using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using DropBear.Codex.TemporalHistory.Interfaces;
using DropBear.Codex.TemporalHistory.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Provides extension methods for ModelBuilder to configure entities for temporal tables and auditing capabilities.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Configures entities marked with TemporalAttribute and IAuditable for temporal table support and auditing.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance.</param>
    /// <param name="configureChangeLog">An optional action to further customize the ChangeLog entity configuration.</param>
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder,
        Action<EntityTypeBuilder<ChangeLog>>? configureChangeLog = null)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            try
            {
                entityType.ClrType.ApplyTemporalConfiguration(modelBuilder);
                entityType.ClrType.ApplyAuditConfiguration(modelBuilder);
            }
            catch (Exception ex)
            {
                // Consider implementing a logging mechanism or allowing this method to throw to the caller.
                Console.WriteLine($"Error applying configurations to {entityType.Name} - {ex.Message}");
            }

        // Configure the ChangeLog entity with an optional custom configuration
        var changeLogBuilder = modelBuilder.Entity<ChangeLog>();
        changeLogBuilder.Property(e => e.ChangeType).HasConversion<string>();
        configureChangeLog?.Invoke(changeLogBuilder);
    }

    private static void ApplyTemporalConfiguration(this Type clrType, ModelBuilder modelBuilder)
    {
        var temporalAttribute = clrType.GetCustomAttribute<TemporalAttribute>();
        if (temporalAttribute is not null)
            modelBuilder.Entity(clrType).ToTable(b => b.IsTemporal(t =>
            {
                t.UseHistoryTable(temporalAttribute.HistoryTableName ?? $"{clrType.Name}History");
                t.HasPeriodStart(temporalAttribute.PeriodStartColumnName ?? "ValidFrom");
                t.HasPeriodEnd(temporalAttribute.PeriodEndColumnName ?? "ValidTo");
            }));
    }

    private static void ApplyAuditConfiguration(this Type clrType, ModelBuilder modelBuilder)
    {
        if (typeof(IAuditable).IsAssignableFrom(clrType)) modelBuilder.Entity(clrType).ConfigureAuditProperties();
    }

    private static void ConfigureAuditProperties(this EntityTypeBuilder entityBuilder)
    {
        entityBuilder.Property<DateTime>("CreatedAt").IsRequired();
        entityBuilder.Property<string>("CreatedBy").IsRequired().HasMaxLength(255);
        entityBuilder.Property<DateTime>("ModifiedAt").IsRequired();
        entityBuilder.Property<string>("ModifiedBy").IsRequired().HasMaxLength(255);
    }
}
