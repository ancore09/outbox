namespace Outbox.Core.Options;

public class GeneratorOptions
{
    public static string Section => nameof(GeneratorOptions);

    public bool Enabled { get; set; }
    public int IntervalSeconds { get; set; }
    public int BatchSize { get; set; }
}