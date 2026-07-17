using BuildingBlocks.Context.Models;

namespace BuildingBlocks.Context.Interfaces;

public interface IRequestContext
{
    string? CorrelationId { get; set; }

    string? RequestId { get; set; }

    string? ClientIp { get; set; }

    string? UserAgent { get; set; }

    CurrentUser User { get; set; }
}