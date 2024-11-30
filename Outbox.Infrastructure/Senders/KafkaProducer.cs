using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Outbox.Core.Models;
using Outbox.Core.Options;
using Outbox.Core.Senders;

namespace Outbox.Infrastructure.Senders;

public class KafkaProducer : IOutboxMessageSender
{
    private readonly SenderOptions _options;
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(IOptionsMonitor<SenderOptions> options)
    {
        _options = options.CurrentValue;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _options.Server,
            ClientId = _options.ClientId,
            // MessageTimeoutMs = config.Value.MessageTimeoutMs,
            // Acks = Acks.Parse(config.Value.Acks),
            // // Enable idempotence for exactly-once delivery semantics
            // EnableIdempotence = true,
            // // Increase reliability
            // MessageSendMaxRetries = 3,
            // RetryBackoffMs = 1000,
            LingerMs = 1,
            // EnableDeliveryReports = true
        };

        _producer = new ProducerBuilder<string, string>(producerConfig)
            .Build();
    }

    public async Task Send(OutboxMessage message)
    {
        var msg = new Message<string, string>()
        {
            Key = message.Key,
            Value = message.Payload
        };
        await _producer.ProduceAsync(message.Topic, msg);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}