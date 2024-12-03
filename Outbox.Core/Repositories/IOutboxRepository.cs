using Outbox.Core.Models;

namespace Outbox.Core.Repositories;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetMessagesWithLock(int batchSize);
    Task<OutboxMessage?> GetFirstMessage();
    Task<List<OutboxMessage>> GetMessages(string topic, int batchSize);
    Task<int> DeleteMessages(List<long> idents);
}