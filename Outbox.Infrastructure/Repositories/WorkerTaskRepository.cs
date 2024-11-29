using System.Data;
using Dapper;
using Npgsql;
using Outbox.Core.Models;
using Outbox.Core.Repositories;

namespace Outbox.Infrastructure.Repositories;

public class WorkerTaskRepository : IWorkerTaskRepository
{
    private readonly NpgsqlConnection _connection;
    private IDbTransaction? _transaction;

    public WorkerTaskRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task BeginTransaction()
    {
        if (_connection.State is ConnectionState.Closed)
            await _connection.OpenAsync();

        _transaction = await _connection.BeginTransactionAsync();
    }

    public void Commit()
    {
        _transaction?.Commit();
    }

    public void Rollback()
    {
        _transaction?.Rollback();
    }

    public async Task<WorkerTask?> GetFirstFreeTaskWithLock()
    {
        var query = """
                    select * from worker_task
                    where lease_end < now()
                    limit 1 
                    for update skip locked;
                    """;

        var workerTask = await _connection.QueryFirstOrDefaultAsync<WorkerTask>(query);

        return workerTask;
    }

    public async Task UpdateLease(long id, DateTimeOffset leaseEnd)
    {
        var query = """
                    update worker_task
                    set lease_end = @leaseEnd
                    where id = @id
                    """;

        await _connection.ExecuteAsync(query, new {id = id, leaseEnd = leaseEnd});
    }
}