using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Outbox.Api.Services;
using Outbox.Core;
using Outbox.Core.Metrics;
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


builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Graylog(new GraylogSinkOptions
        {
            HostnameOrAddress = graylogOptions!.Host,
            Port = graylogOptions.Port,
            TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp,
            // MinimumLogEventLevel = LogEventLevel.Information,
            Facility = "Outbox"
        })
        .WriteTo.Console();
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(serviceName: "Outbox"))
    .WithMetrics(metrics => metrics
        // Add default instrumentation
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        // Add Prometheus exporter
        .AddPrometheusExporter()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        // Add your custom meter
        .AddMeter("Outbox")
    );

builder.Services.AddSingleton<IMetricsContainer, MetricsContainer>();

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
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();