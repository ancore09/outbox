using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outbox.Core;

namespace Outbox.Infrastructure;

public class NewTaskAcquirerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public NewTaskAcquirerBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var serviceProvider = scope.ServiceProvider;

        await StartWork(serviceProvider, stoppingToken);
    }

    private async Task StartWork(IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var service = serviceProvider.GetRequiredService<INewTaskAcquirerService>();

            var result = await service.TryAcquireNewTask();

            if (result)
                continue;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}