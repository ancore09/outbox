using Outbox.Core.Models;

namespace Outbox.Core.Senders;

public interface IOutboxMessageSender : IDisposable
{
    Task Send(OutboxMessage message);
}