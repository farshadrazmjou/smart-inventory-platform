using BuildingBlocks.Logging.Constants;
using BuildingBlocks.Logging.Enrichers;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace BuildingBlocks.Logging.Configuration;

public static class SerilogConfiguration
{
    public static void Configure(
        LoggerConfiguration loggerConfiguration,
        IConfiguration configuration,
        IServiceProvider services)
    {
        var serviceName = configuration["OpenTelemetry:ServiceName"];
        var version = configuration["OpenTelemetry:ServiceVersion"];
        var seqUrl = configuration["Seq:ServerUrl"];

        loggerConfiguration.ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty(name: "ServiceName",value: serviceName)
            .Enrich.WithProperty(name: "ServiceVersion",value: version)
            .Enrich.With<ActivityEnricher>()
            .WriteTo.Console(outputTemplate: LogTemplates.Console)
            .WriteTo.File(
                path: "Logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: LogTemplates.File);

        if (!string.IsNullOrWhiteSpace(value: seqUrl))
        {
            loggerConfiguration.WriteTo.Seq(
                serverUrl: seqUrl, restrictedToMinimumLevel: LogEventLevel.Information);
        }
    }
}