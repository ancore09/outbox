using Microsoft.Extensions.DependencyInjection;
using Outbox.Infrastructure.Leasing;
using Outbox.Infrastructure.Pessimistic;

namespace Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddLeasingBackgroundWorkers();
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
}