using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Outbox.Core.Leasing;
using Outbox.Core.Pessimistic;

namespace Outbox.Infrastructure.Pessimistic;

public class PessemisticBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public PessemisticBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _logger = _serviceProvider.GetRequiredService<ILogger<PessemisticBackgroundService>>();

        _logger.LogInformation("Pessimistic Outbox started");

        var tasks = Enumerable.Range(1, 5).Select(x => StartWork(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task StartWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var scopeServiceProvider = scope.ServiceProvider;

            var service = scopeServiceProvider.GetRequiredService<IPessimisticOutboxProcessor>();

            var result = await service.SendMessages(500);

            if (result)
                continue;

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}