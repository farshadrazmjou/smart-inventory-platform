using BuildingBlocks.Context.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BuildingBlocks.Context.Middleware;

public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IRequestContext context)
    {
        context.CorrelationId = httpContext.TraceIdentifier;
        context.RequestId = httpContext.TraceIdentifier;
        context.ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        context.UserAgent = httpContext.Request.Headers.UserAgent.ToString();

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            context.User.UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            context.User.Username = httpContext.User.FindFirstValue(ClaimTypes.Name);
            context.User.Role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        }

        await _next(httpContext);
    }
}