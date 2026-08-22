using BuildingBlocks.Context.Models;

namespace BuildingBlocks.Context.Interfaces;

public interface IRequestContext
{
    public string? CorrelationId { get; set; }

    public string? RequestId { get; set; }

    public string TraceId { get; set; }

    public string SpanId { get; set; }

    public UserContext User { get; set; }

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public string? RequestPath { get; set; }

    public string? Method { get; set; }
}