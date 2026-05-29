using Serilog.Context;

namespace ProductService.API.Middlewares;

public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.ContainsKey(HeaderName))
        {
            correlationId = context.Request.Headers[HeaderName].ToString();
        }
        else
        {
            correlationId=Guid.NewGuid().ToString();
            context.Request.Headers[HeaderName]=correlationId;
        }

        context.Response.Headers[HeaderName] = correlationId!;

        using (LogContext.PushProperty(
                   "CorrelationId",
                   correlationId.ToString()))
        {
            await _next(context);
        }
    }

}