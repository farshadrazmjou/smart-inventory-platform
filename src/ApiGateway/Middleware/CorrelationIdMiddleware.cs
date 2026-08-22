using System.Diagnostics;
using Serilog.Context;

namespace ApiGateway.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next,ILogger<CorrelationIdMiddleware> logger)
    {
        _next=next;
        _logger=logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var correlationId =
            httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        using (LogContext.PushProperty(name: "CorrelationId", value: correlationId))
        using (LogContext.PushProperty(name: "TraceId",value: Activity.Current?.TraceId.ToString()))
        using (LogContext.PushProperty(name: "SpanId",value: Activity.Current?.SpanId.ToString()))
        using (LogContext.PushProperty(name: "ParentSpanId", value: Activity.Current?.ParentSpanId.ToString()))        
        {
            httpContext.Items["CorrelationId"] = correlationId;
            httpContext.Request.Headers["X-Correlation-Id"] = correlationId;
            httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            await _next(httpContext);
        }
    }
}