using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Core.Leasing;
using Outbox.Core.Models;
using Outbox.Core.Optimistic;
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


        services.Configure<LeasingOptions>(configuration.GetSection(LeasingOptions.Section));
        services.Configure<PessimisticOptions>(configuration.GetSection(PessimisticOptions.Section));
        services.Configure<OptimisticOptions>(configuration.GetSection(OptimisticOptions.Section));

        var outboxOptionsSection = configuration.GetSection(OutboxOptions.Section);

        var outboxOptions = outboxOptionsSection.Get<OutboxOptions>();

        switch (outboxOptions.Type)
        {
            case OutboxType.Leasing:
                services.AddLeasing();
                break;
            case OutboxType.Pessimistic:
                services.AddPessimistic();
                break;
            case OutboxType.Optimistic:
                services.AddOptimistic();
                break;
        }

        services.Configure<OutboxOptions>(outboxOptionsSection);


        // services.AddLeasing();
        // services.AddOptimistic();
        // services.AddPessimistic();

        return services;
    }

    public static IServiceCollection AddLeasing(this IServiceCollection services)
    {
        services.AddScoped<INewTaskAcquirerService, NewTaskAcquirerService>();
        services.AddSingleton<IWorkerTasksContainer, WorkerTasksContainer>();
        services.AddScoped<ILeaseProlongationService, LeaseProlongationService>();
        services.AddScoped<ILeasingOutboxProcessor, LeasingLeasingOutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddPessimistic(this IServiceCollection services)
    {
        services.AddScoped<IPessimisticOutboxProcessor, PessimisticOutboxProcessor>();

        return services;
    }

    public static IServiceCollection AddOptimistic(this IServiceCollection services)
    {
        services.AddScoped<IOptimiticOutboxProcessor, OptimiticOutboxProcessor>();

        return services;
    }
}