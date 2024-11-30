using Microsoft.Extensions.DependencyInjection;

namespace Outbox.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundWorkers(this IServiceCollection services)
    {
        services.AddHostedService<NewTaskAcquirerBackgroundService>();
        services.AddHostedService<LeaseProlongationBackgroundService>();

        return services;
    }
}