using System.Diagnostics.Metrics;
using Outbox.Core.Metrics;

namespace Outbox.Infrastructure.Metrics;

public class MetricsContainer : IMetricsContainer
{
    private readonly Counter<long> _produced;
    private readonly Counter<long> _concurrencyException;
    private readonly Counter<long> _produceTries;

    public MetricsContainer(IMeterFactory factory)
    {
        var producedMeter = factory.Create("Outbox");
        _produced = producedMeter.CreateCounter<long>("outbox_produced", description: "Number of produced messages");
        _concurrencyException = producedMeter.CreateCounter<long>("outbox_concurrency_error", description: "Number of concurrency errors");
        _produceTries = producedMeter.CreateCounter<long>("outbox_produce_tries", description: "Number of produce_tries");
    }

    public void AddProduced()
    {
        _produced.Add(1);
    }
    
    public void AddError()
    {
        _concurrencyException.Add(1);
    }
    
    public void AddProduceTry()
    {
        _produceTries.Add(1);
    }
}