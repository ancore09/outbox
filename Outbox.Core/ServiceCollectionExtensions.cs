using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Core.Leasing;
using Outbox.Core.Options;
using Outbox.Core.Pessimistic;

namespace Outbox.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.Section));
        services.Configure<GraylogOptions>(configuration.GetSection(GraylogOptions.Section));
        services.Configure<SenderOptions>(configuration.GetSection(SenderOptions.Section));

        services.AddScoped<ILeasingOutboxProcessor, LeasingLeasingOutboxProcessor>();

        services.AddLeasing();
        // services.AddPessimistic();

        return services;
    }

    public static IServiceCollection AddLeasing(this IServiceCollection services)
    {
        services.AddScoped<INewTaskAcquirerService, NewTaskAcquirerService>();
        services.AddSingleton<IWorkerTasksContainer, WorkerTasksContainer>();
        services.AddScoped<ILeaseProlongationService, LeaseProlongationService>();

        return services;
    }

    public static IServiceCollection AddPessimistic(this IServiceCollection services)
    {
        services.AddScoped<IPessimisticOutboxProcessor, PessimisticOutboxProcessor>();

        return services;
    }
}