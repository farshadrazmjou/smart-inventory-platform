namespace BuildingBlocks.Observability.Configuration;

public sealed class ServiceInfo
{
    public string ServiceName { get; init; } = default!;
    public string ServiceVersion { get; init; } = default!;
}