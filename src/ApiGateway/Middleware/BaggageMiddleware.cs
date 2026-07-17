using OpenTelemetry;

namespace ApiGateway.Middleware;

public class BaggageMiddleware
{
    private readonly RequestDelegate _next;

    public BaggageMiddleware(RequestDelegate next)
    {
        _next=next;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        var correllationId=httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
            ??Guid.NewGuid().ToString();
        
        Baggage.SetBaggage("correlation.id",correllationId);
        Baggage.SetBaggage("client.ip",httpContext.Connection.RemoteIpAddress?.ToString()??"Unknown");

        // Temp
        foreach (var item in Baggage.GetBaggage())
        {
            Console.WriteLine($"{item.Key} = {item.Value}");
        }

        await _next(httpContext);
    }
}