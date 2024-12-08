using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core.Models;
using Outbox.Core.Options;
using Outbox.Core.Repositories;

namespace Outbox.Core.Generator;

public interface IGeneratorService
{
    Task GenerateOutboxMessages();
}

public class GeneratorService : IGeneratorService
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<GeneratorService> _logger;
    private readonly IOptionsMonitor<GeneratorOptions> _options;
    private static List<OutboxMessage> _messages;

    public GeneratorService(IOutboxRepository outboxRepository, ILogger<GeneratorService> logger, IOptionsMonitor<GeneratorOptions> options)
    {
        _outboxRepository = outboxRepository;
        _logger = logger;
        _options = options;
    }

    public async Task GenerateOutboxMessages()
    {
        if (_options.CurrentValue.Enabled is false)
            return;

        var batchSize = _options.CurrentValue.BatchSize;

        var messages = Enumerable.Range(0, batchSize).AsParallel().WithDegreeOfParallelism(50).Select(CreateMessage).ToList();

        await _outboxRepository.InsertMessages(messages);
        
        _logger.LogInformation("Generated {batchSize} outbox messages", batchSize);
    }

    private OutboxMessage CreateMessage(int index)
    {
        var num = index % 5 + 1;
        var topic = $"test{num}";
        return new OutboxMessage()
        {
            Topic = topic,
            Key = num.ToString(),
            Payload = num.ToString(),
            State = 0
        };
    }
}