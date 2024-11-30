using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Outbox.Core.Options;
using Outbox.Core.Repositories;
using Outbox.Infrastructure.Repositories;

namespace Outbox.Infrastructure.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection(DatabaseOptions.Section).Get<DatabaseOptions>();
        Console.WriteLine(databaseOptions!.ConnectionString);
        services.AddNpgsqlDataSource(databaseOptions!.ConnectionString, builder =>
        {
            builder.UseLoggerFactory(NullLoggerFactory.Instance);
        });

        services.AddScoped<IWorkerTaskRepository, WorkerTaskRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        return services;
    }
}