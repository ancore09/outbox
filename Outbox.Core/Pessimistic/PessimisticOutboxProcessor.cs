using Microsoft.Extensions.Logging;
using Outbox.Core.Metrics;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core.Pessimistic;

public interface IPessimisticOutboxProcessor
{
    Task<bool> SendMessages(int batchSize);
}

public class PessimisticOutboxProcessor : IPessimisticOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;
    private readonly ILogger<PessimisticOutboxProcessor> _logger;
    private readonly IMetricsContainer _metrics;

    public PessimisticOutboxProcessor(IOutboxRepository outboxRepository, IOutboxMessageSender sender, ILogger<PessimisticOutboxProcessor> logger, IMetricsContainer metrics)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _logger = logger;
        _metrics = metrics;
    }


    public async Task<bool> SendMessages(int batchSize)
    {
        var messages = await _outboxRepository.GetMessagesWithLock(batchSize);

        if (messages.Count is 0)
            return false;

        foreach (var outboxMessage in messages)
        {
            // await _outboxRepository.InsertProduced([outboxMessage]);
            // await Task.Delay(1);
            await _sender.Send(outboxMessage);
            _metrics.AddProduced();
        }

        await _outboxRepository.DeleteMessages(messages.Select(x => x.Id).ToList());

        return true;
    }
}