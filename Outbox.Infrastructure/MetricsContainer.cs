using System.Diagnostics.Metrics;
using Outbox.Core.Metrics;

namespace Outbox.Infrastructure;

public class MetricsContainer : IMetricsContainer
{
    private readonly Counter<long> _produced;

    public MetricsContainer(IMeterFactory factory)
    {
        var producedMeter = factory.Create("Outbox");
        _produced = producedMeter.CreateCounter<long>("outbox_produced", description: "Number of produced messages");
    }

    public void AddProduced()
    {
        _produced.Add(1);
    }
}