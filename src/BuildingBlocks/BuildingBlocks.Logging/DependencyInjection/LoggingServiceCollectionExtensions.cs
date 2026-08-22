using BuildingBlocks.Context.Interfaces;
using BuildingBlocks.Context.Models;
using BuildingBlocks.Logging.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Logging.DependencyInjection;

public static class LoggingServiceCollectionExtensions
{
    public static IHostBuilder AddInventoryLogging(this IHostBuilder hostBuilder,IConfiguration configuration)
    {
        hostBuilder.ConfigureServices(configureDelegate: services =>
        {
            services.AddHttpContextAccessor();
        });

        hostBuilder.UseSerilog( configureLogger: (context,services,loggerConfiguration) =>
        {
            SerilogConfiguration.Configure(loggerConfiguration,configuration,services);
        });

        return hostBuilder;
    }
}