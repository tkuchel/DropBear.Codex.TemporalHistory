using System;
using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using DropBear.Codex.TemporalHistory.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
/// Provides extension methods for <see cref="ModelBuilder"/> to apply configurations related to temporal tables.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Applies temporal table configuration to all entities marked with the <see cref="TemporalAttribute"/>.
    /// </summary>
    /// <param name="modelBuilder">The model builder instance.</param>
    public static void ApplyTemporalTableConfiguration(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var temporalAttribute = entityType.ClrType.GetCustomAttribute<TemporalAttribute>();
            if (temporalAttribute != null)
            {
                var entityBuilder = modelBuilder.Entity(entityType.ClrType);

                entityBuilder.ToTable(b => b.IsTemporal(temporalTableBuilder =>
                {
                    temporalTableBuilder.UseHistoryTable(temporalAttribute.HistoryTableName ??
                                                         $"{entityType.ClrType.Name}History");
                    temporalTableBuilder.HasPeriodStart(temporalAttribute.PeriodStartColumnName);
                    temporalTableBuilder.HasPeriodEnd(temporalAttribute.PeriodEndColumnName);
                }));

                // Example for integrating audit columns (CreatedBy, ModifiedBy)
                if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
                {
                    entityBuilder.Property<DateTime>("CreatedAt");
                    entityBuilder.Property<string>("CreatedBy");
                    entityBuilder.Property<DateTime>("ModifiedAt");
                    entityBuilder.Property<string>("ModifiedBy");
                }
            }
        }
    }
}
