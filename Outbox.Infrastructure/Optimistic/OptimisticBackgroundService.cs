using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outbox.Core.Optimistic;
using Outbox.Core.Pessimistic;

namespace Outbox.Infrastructure.Optimistic;

public class OptimisticBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public OptimisticBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _logger = _serviceProvider.GetRequiredService<ILogger<OptimisticBackgroundService>>();

        _logger.LogInformation("Pessimistic Outbox started");

        var tasks = Enumerable.Range(1, 20).Select(x => StartWork(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task StartWork(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var scopeServiceProvider = scope.ServiceProvider;
        var service = scopeServiceProvider.GetRequiredService<IOptimiticOutboxProcessor>();


        while (!stoppingToken.IsCancellationRequested)
        {
            var result = await service.SendMessages();

            if (result)
                continue;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}