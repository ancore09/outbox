namespace Outbox.Core.Metrics;

public interface IMetricsContainer
{
    void AddProduced();
    void AddError();
    void AddProduceTry();
}