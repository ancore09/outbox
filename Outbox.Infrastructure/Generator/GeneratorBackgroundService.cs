using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Outbox.Core.Generator;
using Outbox.Core.Options;

namespace Outbox.Infrastructure.Generator;

public class GeneratorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsMonitor<GeneratorOptions> _options;

    public GeneratorBackgroundService(IServiceProvider serviceProvider, IOptionsMonitor<GeneratorOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.CurrentValue.Enabled is false)
            return;
        
        await StartWork(stoppingToken);
    }

    private async Task StartWork(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var serviceProvider = scope.ServiceProvider;
            
            var service = serviceProvider.GetRequiredService<IGeneratorService>();

            await service.GenerateOutboxMessages();
            
            await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.IntervalSeconds), stoppingToken);
        }
    }
}