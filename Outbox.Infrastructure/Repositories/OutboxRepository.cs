using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Outbox.Core.Models;
using Outbox.Core.Repositories;

namespace Outbox.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly NpgsqlConnection _connection;
    private readonly ILogger<OutboxRepository> _logger;

    public OutboxRepository(NpgsqlConnection connection, ILogger<OutboxRepository> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task<List<OutboxMessage>> GetMessagesWithLock(int batchSize)
    {
        if (_connection.State is not ConnectionState.Open)
            await _connection.OpenAsync();

        await using var transaction = await _connection.BeginTransactionAsync();

        var selectQuery = """
                    select * from outbox
                    where state = 0
                    order by id
                    limit @batchSize
                    for update skip locked;
                    """;

        var result = (await _connection.QueryAsync<OutboxMessage>(selectQuery, new { batchSize = batchSize })).ToList();

        var updateQuery = """
                          update outbox
                          set state = 1
                          where id = ANY(@idents)
                          """;

        await _connection.ExecuteAsync(updateQuery, new { idents = result.Select(x => x.Id).ToArray() });

        await transaction.CommitAsync();

        return result;
    }

    public async Task<OutboxMessage?> GetFirstMessage()
    {
        var selectQuery = """
                          select *, xmin from outbox
                          where state = 0
                          order by id
                          limit 1;
                          """;

        var result = await _connection.QueryFirstOrDefaultAsync<OutboxMessage>(selectQuery);

        if (result is null)
            return null;

        var updateQuery = """
                          update outbox
                          set state = 1
                          where id = @id and state = 0 and xmin = @xmin
                          """;

        var updated = await _connection.ExecuteAsync(updateQuery, new { id = result.Id, xmin = result.Xmin });

        if (updated is 0)
        {
            _logger.LogInformation("Optimistic concurrency exception");
            return null;
        }

        return result;
    }

    public async Task<List<OutboxMessage>> GetMessages(string topic, int batchSize)
    {
        var query = """
                    select * from outbox
                    where topic = @topic
                    order by id
                    limit @batchSize;
                    """;

        var result = await _connection.QueryAsync<OutboxMessage>(query, new { topic = topic, batchSize = batchSize });

        return result.ToList();
    }

    public async Task<int> DeleteMessages(List<long> idents)
    {
        var query = """
                    delete from outbox
                    where id = ANY (@idents);
                    """;

        var result = await _connection.ExecuteAsync(query, new { idents = idents });

        return result;
    }

    public async Task<int> InsertProduced(List<OutboxMessage> messages)
    {
        var query = """
                    insert into produced (topic, payload)
                    values (@Topic, @Payload)
                    """;

        var result = await _connection.ExecuteAsync(query, messages);

        return result;
    }
}