namespace Outbox.Core.Options;

public class GraylogOptions
{
    public static string Section => nameof(GraylogOptions);

    public required string Host { get; set; }
    public required int Port { get; set; }
}