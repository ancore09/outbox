using Microsoft.Extensions.DependencyInjection;
using Outbox.Core.Options;

namespace Outbox.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        services.AddOptions<DatabaseOptions>(DatabaseOptions.Section);

        services.AddScoped<INewTaskAcquirerService, NewTaskAcquirerService>();
        services.AddSingleton<IWorkerTasksContainer, WorkerTasksContainer>();

        return services;
    }
}