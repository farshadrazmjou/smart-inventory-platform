namespace BuildingBlocks.Observability.Metrics;

public static class MetricNames
{
    public const string MeterName = "InventorySystem";

    public const string HttpRequests = "inventory.http.requests";

    public const string HttpRequestDuration = "inventory.http.duration";

    public const string Exceptions = "inventory.exceptions";

    public const string DatabaseQueries = "inventory.database.queries";

    public const string PublishedEvents = "inventory.events.published";

    public const string ConsumedEvents = "inventory.events.consumed";
}