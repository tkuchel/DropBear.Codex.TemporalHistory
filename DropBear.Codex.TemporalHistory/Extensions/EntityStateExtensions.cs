using DropBear.Codex.TemporalHistory.Enums;
using Microsoft.EntityFrameworkCore;

namespace DropBear.Codex.TemporalHistory.Extensions;

public static class EntityStateExtensions
{
    /// <summary>
    ///     Converts an EntityState to a ChangeTypeEnum.
    /// </summary>
    /// <param name="state">The EntityState to convert.</param>
    /// <returns>The corresponding ChangeTypeEnum.</returns>
    public static ChangeTypeEnum ToChangeTypeEnum(this EntityState state) =>
        state switch
        {
            EntityState.Added => ChangeTypeEnum.Added,
            EntityState.Modified => ChangeTypeEnum.Updated,
            EntityState.Deleted => ChangeTypeEnum.Deleted,
            _ => ChangeTypeEnum.NotAvailable
        };
}
