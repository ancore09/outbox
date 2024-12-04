using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core.Optimistic;
using Outbox.Core.Options;
using Outbox.Core.Pessimistic;

namespace Outbox.Infrastructure.Optimistic;

public class OptimisticBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<OptimisticOptions> _options;

    public OptimisticBackgroundService(IServiceProvider serviceProvider, IOptionsMonitor<OptimisticOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var _logger = _serviceProvider.GetRequiredService<ILogger<OptimisticBackgroundService>>();

        _logger.LogInformation("Optimistic Outbox started");

        var tasks = Enumerable.Range(1, _options.CurrentValue.Workers).Select(x => StartWork(stoppingToken)).ToList();

        await Task.WhenAll(tasks);
    }

    private async Task StartWork(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(_options.CurrentValue.ThrottlingMilliseconds));

            using var scope = _serviceProvider.CreateScope();
            var scopeServiceProvider = scope.ServiceProvider;
            var service = scopeServiceProvider.GetRequiredService<IOptimiticOutboxProcessor>();

            var result = await service.SendMessages();

            if (result)
                continue;

            await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.DelaySeconds), stoppingToken);
        }
    }
}