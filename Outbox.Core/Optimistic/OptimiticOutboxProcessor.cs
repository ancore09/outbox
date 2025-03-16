using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core.Metrics;
using Outbox.Core.Options;
using Outbox.Core.Pessimistic;
using Outbox.Core.Repositories;
using Outbox.Core.Senders;

namespace Outbox.Core.Optimistic;

public interface IOptimiticOutboxProcessor
{
    Task<bool> SendMessages();
    Task<bool> SendMessages(int reminder);
}

public class OptimiticOutboxProcessor : IOptimiticOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IOutboxMessageSender _sender;
    private readonly ILogger<OptimiticOutboxProcessor> _logger;
    private readonly IMetricsContainer _metrics;
    private readonly IOptionsMonitor<OptimisticOptions> _options;

    public OptimiticOutboxProcessor(IOutboxRepository outboxRepository, IOutboxMessageSender sender, ILogger<OptimiticOutboxProcessor> logger, IMetricsContainer metrics, IOptionsMonitor<OptimisticOptions> options)
    {
        _outboxRepository = outboxRepository;
        _sender = sender;
        _logger = logger;
        _metrics = metrics;
        _options = options;
    }


    public async Task<bool> SendMessages()
    {
        var outboxMessage = await _outboxRepository.GetFirstMessage(_options.CurrentValue.RandomRange);
        _metrics.AddProduceTry();

        if (outboxMessage is null)
        {
            _metrics.AddError();
            return true;
        }

        await _sender.Send(outboxMessage);

        await _outboxRepository.DeleteMessagesByIdAndState([outboxMessage.Id]);

        return true;
    }

    public async Task<bool> SendMessages(int reminder)
    {
        var outboxMessage = await _outboxRepository.GetFirstMessageByReminder(_options.CurrentValue.RandomRange, _options.CurrentValue.Reminders, reminder);
        _metrics.AddProduceTry();

        if (outboxMessage is null)
        {
            _metrics.AddError();
            return true;
        }

        await _sender.Send(outboxMessage);
        _metrics.AddProduced();

        await _outboxRepository.DeleteMessagesByIdAndState([outboxMessage.Id]);

        return true;
    }
}