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