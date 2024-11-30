using Outbox.Core.Models;

namespace Outbox.Core.Repositories;

public interface IOutboxRepository
{
    Task<List<OutboxMessage>> GetMessages(string topic, int batchSize);
    Task<int> DeleteMessages(List<long> idents);
    Task<int> InsertProduced(List<OutboxMessage> messages);
}