namespace Outbox.Core.Models;

public class OutboxMessage
{
    public long Id { get; set; }
    public required string Topic { get; set; }
    public required string Key { get; set; }
    public required string Payload { get; set; }

    public required int State { get; set; } // 0 - new, 1 - sending
}