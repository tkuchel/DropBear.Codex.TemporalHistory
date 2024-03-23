namespace DropBear.Codex.TemporalHistory.Models;

public class TemporalRecord<T> where T : class
{
    public T? Entity { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}
