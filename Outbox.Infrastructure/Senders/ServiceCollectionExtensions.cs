using Microsoft.Extensions.DependencyInjection;
using Outbox.Core.Senders;

namespace Outbox.Infrastructure.Senders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProducers(this IServiceCollection services)
    {
        services.AddSingleton<IOutboxMessageSender, KafkaProducer>();

        return services;
    }
}