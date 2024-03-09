namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
///     Indicates that a property should not be included in audit logs.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
// ReSharper disable once ClassNeverInstantiated.Global
public sealed class DoNotLogAttribute : Attribute;

