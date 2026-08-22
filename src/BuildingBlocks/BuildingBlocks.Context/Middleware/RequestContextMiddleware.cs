using BuildingBlocks.Context.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Serilog.Context;
using System.Diagnostics;

namespace BuildingBlocks.Context.Middleware;

public sealed class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IRequestContext requestContext)
    {
        requestContext.CorrelationId = httpContext.TraceIdentifier;
        requestContext.RequestId = httpContext.TraceIdentifier;
        requestContext.ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        requestContext.UserAgent = httpContext.Request.Headers.UserAgent.ToString();

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            requestContext.User.UserId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            requestContext.User.Username = httpContext.User.FindFirstValue(ClaimTypes.Name);
            foreach(var role in httpContext.User.FindAll(ClaimTypes.Role))
                requestContext.User.Roles.Add(role.Value);
        }

        var activity = Activity.Current;
        
        using (LogContext.PushProperty("TraceId", activity?.TraceId.ToString()))
        using (LogContext.PushProperty("SpanId", activity?.SpanId.ToString()))
        using (LogContext.PushProperty("CorrelationId", requestContext.CorrelationId))
        using (LogContext.PushProperty("RequestId", requestContext.RequestId))
        using (LogContext.PushProperty("ClientIp", requestContext.ClientIp))
        using (LogContext.PushProperty("UserAgent", requestContext.UserAgent))
        using (LogContext.PushProperty("UserId", requestContext.User.UserId))
        using (LogContext.PushProperty("Username", requestContext.User.Username))
        using (LogContext.PushProperty("Role", requestContext.User.Roles))
        {
            await _next(httpContext);
        }
    }
}