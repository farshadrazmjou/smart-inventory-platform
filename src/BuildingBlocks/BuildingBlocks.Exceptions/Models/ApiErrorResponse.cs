namespace BuildingBlocks.Exceptions.Models;

public sealed class ApiErrorResponse
{
    public int StatusCode { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string TraceId { get; set; } = string.Empty;

    public string? CorrelationId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}