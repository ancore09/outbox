using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Outbox.Core.Leasing;

namespace Outbox.Infrastructure.Leasing;

public class LeaseProlongationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public LeaseProlongationBackgroundService(IServiceProvider serviceProvider)
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
            var service = serviceProvider.GetRequiredService<ILeaseProlongationService>();

            await service.TryProlongLeases();

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}