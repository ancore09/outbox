using System.Data;
using Dapper;
using Npgsql;
using Outbox.Core.Models;
using Outbox.Core.Repositories;

namespace Outbox.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly NpgsqlConnection _connection;

    public OutboxRepository(NpgsqlConnection connection)
    {
        _connection = connection;
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