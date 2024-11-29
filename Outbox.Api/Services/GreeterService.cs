using Grpc.Core;

namespace Outbox.Api.Services;

public class GreeterService : Greeter.GreeterBase
{
    private readonly ILogger<GreeterService> _logger;
    private readonly IConfiguration _configuration;

    public GreeterService(ILogger<GreeterService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        Console.WriteLine(_configuration.GetSection("Database").GetValue<string>("ConnectionString"));
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}