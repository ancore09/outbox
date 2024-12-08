namespace Outbox.Core.Models;

public enum OutboxType
{
    None,
    Leasing,
    Pessimistic,
    Optimistic
}