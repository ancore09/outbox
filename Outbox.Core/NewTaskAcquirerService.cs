using Microsoft.Extensions.Logging;
using Outbox.Core.Repositories;

namespace Outbox.Core;

public interface INewTaskAcquirerService
{
    Task<bool> TryAcquireNewTask();
}

public class NewTaskAcquirerService : INewTaskAcquirerService
{
    private readonly IWorkerTaskRepository _workerTaskRepository;
    private readonly IWorkerTasksContainer _container;
    private readonly ILogger<NewTaskAcquirerService> _logger;

    public NewTaskAcquirerService(IWorkerTaskRepository workerTaskRepository, IWorkerTasksContainer container, ILogger<NewTaskAcquirerService> logger)
    {
        _workerTaskRepository = workerTaskRepository;
        _container = container;
        _logger = logger;
    }

    public async Task<bool> TryAcquireNewTask()
    {
        await _workerTaskRepository.BeginTransaction();

        try
        {
            var workerTask = await _workerTaskRepository.GetFirstFreeTaskWithLock();

            if (workerTask is null)
            {
                _workerTaskRepository.Rollback();
                return false;
            }

            var leaseEnd = DateTimeOffset.UtcNow.AddMinutes(Constants.LeaseDurationMinutes);

            await _workerTaskRepository.UpdateLease(workerTask.Id, leaseEnd);

            _workerTaskRepository.Commit();

            await _container.AddOrUpdateTask(workerTask);

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Exception during acquiring new task");
            _workerTaskRepository.Rollback();
        }

        return false;
    }
}