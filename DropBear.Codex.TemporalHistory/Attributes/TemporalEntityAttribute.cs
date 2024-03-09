namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
///     Indicates that an entity is a temporal entity and should be treated accordingly in the database context
///     configuration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class TemporalEntityAttribute : Attribute;
