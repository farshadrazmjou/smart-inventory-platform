using BuildingBlocks.Observability.Configuration;
using BuildingBlocks.Observability.Factories;
using BuildingBlocks.Observability.Metrics;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability.DependencyInjection;

public static class ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryObservability(
        this IServiceCollection services,
        string serviceName,
        string serviceVersion,
        string otlpEndpoint,
        params string[] activitySources)
    {
        services.AddSingleton(new ServiceInfo
        {
            ServiceName = serviceName,
            ServiceVersion = serviceVersion
        });
        
        services.AddSingleton<IActivityFactory, ActivityFactory>();
        
        services
            .AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService(serviceName, serviceVersion);
            })
            .WithTracing(tracing =>
            {
                foreach (var source in activitySources)
                {
                    tracing.AddSource(source);
                }

                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(MetricNames.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();

                // Prometheus بعداً
            });

        return services;
    }
}