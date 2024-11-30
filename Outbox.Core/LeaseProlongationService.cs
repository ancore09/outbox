using Microsoft.Extensions.Logging;
using Outbox.Core.Repositories;

namespace Outbox.Core;

public interface ILeaseProlongationService
{
    Task TryProlongLeases();
}

public class LeaseProlongationService : ILeaseProlongationService
{
    private readonly IWorkerTaskRepository _workerTaskRepository;
    private readonly IWorkerTasksContainer _container;
    private readonly ILogger<LeaseProlongationService> _logger;

    public LeaseProlongationService(IWorkerTaskRepository workerTaskRepository, IWorkerTasksContainer container, ILogger<LeaseProlongationService> logger)
    {
        _workerTaskRepository = workerTaskRepository;
        _container = container;
        _logger = logger;
    }

    public async Task TryProlongLeases()
    {
        var tasks = await _container.GetWorkerTasks();

        var tasksToProlong = tasks
            .Where(x => DateTimeOffset.UtcNow.AddSeconds(15) > x.LeaseEnd)
            .ToList();

        var updateQuery = tasksToProlong.
            Select(x => (x, x.LeaseEnd!.Value.AddMinutes(Constants.LeaseDurationMinutes)))
            .ToList();


        await _workerTaskRepository.UpdateLeases(updateQuery);

        foreach (var (workerTask, leaseEnd) in updateQuery)
            workerTask.LeaseEnd = leaseEnd;
    }
}