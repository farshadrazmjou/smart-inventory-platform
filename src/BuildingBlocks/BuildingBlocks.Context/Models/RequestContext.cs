using BuildingBlocks.Context.Interfaces;

namespace BuildingBlocks.Context.Models;

public class RequestContext : IRequestContext
{
    public string? CorrelationId { get; set; } = "";

    public string? RequestId { get; set; } = "";

    public string TraceId { get; set; } = "";

    public string SpanId { get; set; } = "";

    public UserContext User { get; set; } = new();

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public string? RequestPath { get; set; }

    public string? Method { get; set; }

}