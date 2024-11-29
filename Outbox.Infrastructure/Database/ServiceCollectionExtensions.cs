using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Outbox.Core.Options;
using Outbox.Core.Repositories;
using Outbox.Infrastructure.Repositories;

namespace Outbox.Infrastructure.Database;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>();
        Console.WriteLine(databaseOptions!.ConnectionString);
        services.AddNpgsqlDataSource(databaseOptions!.ConnectionString);

        services.AddScoped<IWorkerTaskRepository, WorkerTaskRepository>();
        
        return services;
    }
}