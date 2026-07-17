using BuildingBlocks.Logging.Configuration;
using BuildingBlocks.Logging.Interfaces;
using BuildingBlocks.Logging.Services;
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
            services.AddScoped<IUserContextAccessor,UserContextAccessor>();
        });

        hostBuilder.UseSerilog( configureLogger: (context,services,loggerConfiguration) =>
        {
            SerilogConfiguration.Configure(loggerConfiguration,configuration,services);
        });

        return hostBuilder;
    }
}