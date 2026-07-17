using BuildingBlocks.Logging.Constants;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace BuildingBlocks.Logging.Extensions;

public static class LoggingApplicationBuilderExtensions
{
    public static WebApplication UseInventoryLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext =
                (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set(
                        LogPropertyNames.RequestPath,
                        httpContext.Request.Path);

                    diagnosticContext.Set(
                        LogPropertyNames.RequestMethod,
                        httpContext.Request.Method);
                    diagnosticContext.Set(
                        LogPropertyNames.ClientIP,
                        httpContext.Connection.RemoteIpAddress?.ToString());

                    diagnosticContext.Set(
                        LogPropertyNames.UserAgent,
                        httpContext.Request.Headers.UserAgent.ToString());

                    diagnosticContext.Set(
                        LogPropertyNames.Host,
                        httpContext.Request.Host.ToString());

                    diagnosticContext.Set(
                        LogPropertyNames.Scheme,
                        httpContext.Request.Scheme);
                };
        });



        return app;
    }
}