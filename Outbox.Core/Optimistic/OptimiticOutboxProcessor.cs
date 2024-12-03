using Microsoft.Extensions.Logging;
using Outbox.Core.Metrics;
using Outbox.Core.Pessimistic;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core.Optimistic;

public interface IOptimiticOutboxProcessor
{
    Task<bool> SendMessages();
}

public class OptimiticOutboxProcessor : IOptimiticOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;
    private readonly ILogger<OptimiticOutboxProcessor> _logger;
    private readonly IMetricsContainer _metrics;

    public OptimiticOutboxProcessor(IOutboxRepository outboxRepository, IOutboxMessageSender sender, ILogger<OptimiticOutboxProcessor> logger, IMetricsContainer metrics)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _logger = logger;
        _metrics = metrics;
    }


    public async Task<bool> SendMessages()
    {
        var outboxMessage = await _outboxRepository.GetFirstMessage();

        if (outboxMessage is null)
            return false;

        await _sender.Send(outboxMessage);
        _metrics.AddProduced();

        await _outboxRepository.DeleteMessages([outboxMessage.Id]);

        return true;
    }
}