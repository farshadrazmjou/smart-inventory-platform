using System.Diagnostics.Metrics;

namespace BuildingBlocks.Observability.Metrics;

public static class InventoryMeter
{
    public static readonly Meter Meter =
        new(MetricNames.MeterName, "1.0.0");

    public static readonly Counter<long> HttpRequests =
        Meter.CreateCounter<long>(MetricNames.HttpRequests, unit: "requests");

    public static readonly Histogram<double> HttpRequestDuration =
        Meter.CreateHistogram<double>(MetricNames.HttpRequestDuration, unit: "ms");

    public static readonly Counter<long> Exceptions =
        Meter.CreateCounter<long>(MetricNames.Exceptions);

    public static readonly Counter<long> DatabaseQueries =
        Meter.CreateCounter<long>(MetricNames.DatabaseQueries);

    public static readonly Counter<long> PublishedEvents =
        Meter.CreateCounter<long>(MetricNames.PublishedEvents);

    public static readonly Counter<long> ConsumedEvents =
        Meter.CreateCounter<long>(MetricNames.ConsumedEvents);
}