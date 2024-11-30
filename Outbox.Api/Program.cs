using Outbox.Api.Services;
using Outbox.Core;
using Outbox.Core.Options;
using Outbox.Infrastructure;
using Outbox.Infrastructure.Database;
using Outbox.Infrastructure.Senders;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddYamlFile("config.yaml", optional: false, reloadOnChange: true)
    .AddYamlFile($"config.{builder.Environment.EnvironmentName}.yaml", optional: true, reloadOnChange: true);

var graylogOptions = builder.Configuration.GetSection(GraylogOptions.Section).Get<GraylogOptions>();

Log.Logger = new LoggerConfiguration()
    // .MinimumLevel.Information()
    // .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Graylog(new GraylogSinkOptions
    {
        HostnameOrAddress = graylogOptions!.Host,
        Port = graylogOptions.Port,
        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp,
        // MinimumLogEventLevel = LogEventLevel.Information,
        Facility = "Outbox"
    })
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddBackgroundWorkers();
builder.Services.AddProducers();
builder.Services.AddCore(builder.Configuration);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcReflectionService();
app.MapGrpcService<GreeterService>();
app.MapGet("/",
    () =>
        "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();