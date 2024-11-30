using Outbox.Core.Models;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core;

public interface IOutboxSenderService
{
    Task SendMessages(WorkerTask config);
}

public class OutboxSenderService : IOutboxSenderService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;

    public OutboxSenderService(IOutboxRepository outboxRepository, IOutboxMessageSender sender)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
    }

    public async Task SendMessages(WorkerTask config)
    {
        if (config.IsLeaseExpired())
            return;
        
        var messages = await _outboxRepository.GetMessages(config.Topic, config.BatchSize);
        
        if (messages.Count is 0)
            return;

        foreach (var outboxMessage in messages)
        {
            // await _outboxRepository.InsertProduced([outboxMessage]);
            // await Task.Delay(1);
            await _sender.Send(outboxMessage);
        }

        await _outboxRepository.DeleteMessages(messages.Select(x => x.Id).ToList());
    }
}