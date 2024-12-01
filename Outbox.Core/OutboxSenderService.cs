using Microsoft.Extensions.Logging;
using Outbox.Core.Metrics;
using Outbox.Core.Models;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core;

public interface IOutboxSenderService
{
    Task<bool> SendMessages(WorkerTask config);
}

public class OutboxSenderService : IOutboxSenderService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;
    private readonly ILogger<OutboxSenderService> _logger;
    private readonly IMetricsContainer _metrics;

    public OutboxSenderService(IOutboxRepository outboxRepository, IOutboxMessageSender sender, ILogger<OutboxSenderService> logger, IMetricsContainer metrics)
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

        await _outboxRepository.DeleteMessages(messages.Select(x => x.Id).ToList());

        return true;
    }
}