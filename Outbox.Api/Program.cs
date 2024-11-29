using Outbox.Api.Services;
using Outbox.Core;
using Outbox.Infrastructure;
using Outbox.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddYamlFile("config.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"config.{builder.Environment.EnvironmentName}.yaml", optional: true);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddBackgroundWorkers();
builder.Services.AddCore();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcReflectionService();
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();