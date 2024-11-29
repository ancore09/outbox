using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Outbox.Core.Models;

namespace Outbox.Core;

public interface IWorkerTasksContainer : IDisposable
{
    Task AddOrUpdateTask(WorkerTask config);
    Task CancelAndRemoveTask(string topic);
}

public class WorkerTasksContainer : IWorkerTasksContainer
{
    private readonly ConcurrentDictionary<string, (WorkerTask Config, Task Task, CancellationTokenSource Cts)> _container = new();
    private readonly ILogger<WorkerTasksContainer> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationTokenSource _globalCts = new();

    public WorkerTasksContainer(ILogger<WorkerTasksContainer> logger)
    {
        _logger = logger;
    }

    public async Task AddOrUpdateTask(WorkerTask config)
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_container.TryGetValue(config.Topic, out var existing))
            {
                await existing.Cts.CancelAsync();
                await existing.Task;
            }

            var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token);
                
            var task = Task.Run(async () =>
            {
                try
                {
                    while (!cts.Token.IsCancellationRequested)
                    {
                        await ProcessTask(config);
                        await Task.Delay(config.DelayMilliseconds, cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Task {Topic} cancelled", config.Topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing task {Topic}", config.Topic);
                }
            }, cts.Token);

            _container.TryAdd(config.Topic, (config, task, cts));
        }
        finally
        {
            _semaphore.Release();
        }
    }
    
    private async Task ProcessTask(WorkerTask config)
    {
        // Implementation of task processing logic
    }

    public async Task CancelAndRemoveTask(string topic)
    {
        if (_container.TryRemove(topic, out var existing))
        {
            await existing.Cts.CancelAsync();
        }
    }
    
    public void Dispose()
    {
        // _leaseCheckTimer.Dispose();
        _globalCts.Cancel();
        _semaphore.Dispose();
        foreach (var (_, (_, cts, _)) in _container)
        {
            cts.Dispose();
        }
        _globalCts.Dispose();
    }
}