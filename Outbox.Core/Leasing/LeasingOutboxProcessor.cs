using Microsoft.Extensions.Logging;
using Outbox.Core.Metrics;
using Outbox.Core.Models;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core.Leasing;

public interface ILeasingOutboxProcessor
{
    Task<bool> SendMessages(WorkerTask config);
}

public class LeasingLeasingOutboxProcessor : ILeasingOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;
    private readonly ILogger<LeasingLeasingOutboxProcessor> _logger;
    private readonly IMetricsContainer _metrics;

    public LeasingLeasingOutboxProcessor(IOutboxRepository outboxRepository, IOutboxMessageSender sender, ILogger<LeasingLeasingOutboxProcessor> logger, IMetricsContainer metrics)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task<bool> SendMessages(WorkerTask config)
    {
        if (config.IsLeaseExpired())
            return false;

        var messages = await _outboxRepository.GetMessages(config.Topic, config.BatchSize);

        if (messages.Count is 0)
            return false;

        foreach (var outboxMessage in messages)
        {
            // await _outboxRepository.InsertProduced([outboxMessage]);
            // await Task.Delay(1);
            await _sender.Send(outboxMessage);
            _metrics.AddProduced();
        }

        await _outboxRepository.DeleteMessagesByIdAndTopic(messages.Select(x => x.Id).ToList(), config.Topic);

        return true;
    }
}