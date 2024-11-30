using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Outbox.Core.Options;

namespace Outbox.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.Section));
        services.Configure<GraylogOptions>(configuration.GetSection(GraylogOptions.Section));
        services.Configure<SenderOptions>(configuration.GetSection(SenderOptions.Section));

        services.AddScoped<INewTaskAcquirerService, NewTaskAcquirerService>();
        services.AddSingleton<IWorkerTasksContainer, WorkerTasksContainer>();
        services.AddScoped<IOutboxSenderService, OutboxSenderService>();
        services.AddScoped<ILeaseProlongationService, LeaseProlongationService>();

        return services;
    }
}