using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core.Leasing;
using Outbox.Core.Options;
using Outbox.Core.Pessimistic;

namespace Outbox.Infrastructure.Pessimistic;

public class PessemisticBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<PessimisticOptions> _options;

    public PessemisticBackgroundService(IServiceProvider serviceProvider, IOptionsMonitor<PessimisticOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _logger = _serviceProvider.GetRequiredService<ILogger<PessemisticBackgroundService>>();

        _logger.LogInformation("Pessimistic Outbox started");

        var tasks = Enumerable.Range(1, _options.CurrentValue.Workers).Select(x => StartWork(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task StartWork(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_options.CurrentValue.ThrottlingMilliseconds));

            await using var scope = _serviceProvider.CreateAsyncScope();
            var scopeServiceProvider = scope.ServiceProvider;

            var service = scopeServiceProvider.GetRequiredService<IPessimisticOutboxProcessor>();

            var result = await service.SendMessages(_options.CurrentValue.BatchSize);

            if (result)
                continue;

            await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.DelaySeconds), stoppingToken);
        }
    }
}