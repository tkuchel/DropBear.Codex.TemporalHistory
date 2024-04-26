using System.Reflection;
using DropBear.Codex.TemporalHistory.Attributes;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

/// <summary>
///     Extensions for ModelBuilder to configure the temporal behavior of entities.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Configures temporal tables for all entities in the model that are marked with TemporalEntityAttribute.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder to apply configurations to.</param>
    public static void UseTemporalTables(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var attribute = entityType.ClrType.GetCustomAttribute<TemporalEntityAttribute>();
            if (attribute is not null)
                modelBuilder.Entity(entityType.ClrType)
                    .ToTable(GetHistoryTableName(entityType.ClrType), t => t.IsTemporal());
        }
    }

    /// <summary>
    ///     Determines the history table name for a temporal entity.
    /// </summary>
    /// <param name="entityType">The CLR type of the entity.</param>
    /// <returns>The history table name based on the entity type.</returns>
    private static string GetHistoryTableName(Type entityType) =>
        // Implement custom logic for determining history table names
        // This could be based on conventions, attributes, or external configuration
        $"{entityType.Name}History";
}
