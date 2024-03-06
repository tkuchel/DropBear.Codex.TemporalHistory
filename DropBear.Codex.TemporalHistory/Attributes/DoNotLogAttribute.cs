using System;

namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
/// Indicates that a property should not be included in audit logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DoNotLogAttribute : Attribute
{
}
