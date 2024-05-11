using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using DropBear.Codex.TemporalHistory.Attributes;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class ModelBuilderExtensions
{
    public static void UseTemporalTables(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var attribute = entityType.ClrType.GetCustomAttribute<TemporalEntityAttribute>();
            if (attribute != null)
            {
                try
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .ToTable(attribute.HistoryTableName ?? GetHistoryTableName(entityType.ClrType), t => t.IsTemporal());
                }
                catch (Exception ex)
                {
                    // Log the error or handle it accordingly
                    throw new InvalidOperationException($"Failed to configure temporal table for {entityType.ClrType.Name}: {ex.Message}", ex);
                }
            }
        }
    }

    private static string GetHistoryTableName(Type entityType) => $"{entityType.Name}History";
}
