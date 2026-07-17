using BuildingBlocks.Context.Interfaces;
using BuildingBlocks.Context.Models;

namespace BuildingBlocks.Context.Services;

public class RequestContext : IRequestContext
{
    public string? CorrelationId { get; set; }

    public string? RequestId { get; set; }

    public string? ClientIp { get; set; }

    public string? UserAgent { get; set; }

    public CurrentUser User { get; set; } = new();
}