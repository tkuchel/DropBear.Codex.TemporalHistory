namespace DropBear.Codex.TemporalHistory.Attributes;

/// <summary>
///     Attribute to designate a class as a temporal entity.
///     This attribute is used to mark classes that should be tracked
///     for historical changes over time.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TemporalEntityAttribute : Attribute
{
}
