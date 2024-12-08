using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Core;
using Outbox.Core.Models;
using Outbox.Core.Options;
using Outbox.Infrastructure.Generator;
using Outbox.Infrastructure.Leasing;
using Outbox.Infrastructure.Optimistic;
using Outbox.Infrastructure.Pessimistic;

namespace Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        var outboxOptionsSection = configuration.GetSection(OutboxOptions.Section);

        var outboxOptions = outboxOptionsSection.Get<OutboxOptions>();

        if (outboxOptions is not null)
        {
            switch (outboxOptions.Type)
            {
                case OutboxType.Leasing:
                    services.AddLeasingBackgroundWorkers();
                    break;
                case OutboxType.Pessimistic:
                    services.AddPessimisticBackgroundWorkers();
                    break;
                case OutboxType.Optimistic:
                    services.AddOptimisticBackgroundWorkers();
                    break;
            }
        }
        
        services.AddHostedService<GeneratorBackgroundService>();

        // services.AddLeasingBackgroundWorkers();
        // services.AddOptimisticBackgroundWorkers();
        // services.AddPessimisticBackgroundWorkers();

        return services;
    }

    public static IServiceCollection AddLeasingBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<NewTaskAcquirerBackgroundService>();
        services.AddHostedService<LeaseProlongationBackgroundService>();

        return services;
    }

    public static IServiceCollection AddPessimisticBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<PessemisticBackgroundService>();

        return services;
    }

    public static IServiceCollection AddOptimisticBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<OptimisticBackgroundService>();

        return services;
    }
}