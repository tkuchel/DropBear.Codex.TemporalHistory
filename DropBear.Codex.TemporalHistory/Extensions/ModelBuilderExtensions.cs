using System;
using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using DropBear.Codex.TemporalHistory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ModelBuilder"/> to apply configurations related to temporal tables and auditing.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies configuration for entities marked with <see cref="TemporalAttribute"/> and implements <see cref="IAuditable"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ApplyTemporalConfiguration(modelBuilder, entityType);
            ApplyAuditConfiguration(modelBuilder, entityType);
        }
    }

    private static void ApplyTemporalConfiguration(ModelBuilder modelBuilder, Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        var temporalAttribute = entityType.ClrType.GetCustomAttribute<TemporalAttribute>();
        if (temporalAttribute != null)
        {
            modelBuilder.Entity(entityType.ClrType).ToTable(b => b.IsTemporal(temporalTableBuilder =>
            {
                temporalTableBuilder.UseHistoryTable(temporalAttribute.HistoryTableName ?? $"{entityType.ClrType.Name}History");
                temporalTableBuilder.HasPeriodStart(temporalAttribute.PeriodStartColumnName ?? "ValidFrom");
                temporalTableBuilder.HasPeriodEnd(temporalAttribute.PeriodEndColumnName ?? "ValidTo");
            }));
        }
    }

    private static void ApplyAuditConfiguration(ModelBuilder modelBuilder, Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
    {
        if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
        {
            var entityBuilder = modelBuilder.Entity(entityType.ClrType);
            entityBuilder.Property<DateTime>("CreatedAt");
            entityBuilder.Property<string>("CreatedBy").HasMaxLength(255);
            entityBuilder.Property<DateTime>("ModifiedAt");
            entityBuilder.Property<string>("ModifiedBy").HasMaxLength(255);
        }
    }
}
