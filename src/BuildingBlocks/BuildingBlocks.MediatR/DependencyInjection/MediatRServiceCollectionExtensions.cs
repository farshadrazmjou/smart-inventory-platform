using BuildingBlocks.MediatR.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.MediatR.DependencyInjection;

public static class MediatRServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryMediatRBehavior(this IServiceCollection services)
    {
        services.AddTransient(
            serviceType: typeof(IPipelineBehavior<,>),
            implementationType: typeof(TracingBehavior<,>));

        services.AddTransient(
            serviceType: typeof(IPipelineBehavior<,>),
            implementationType: typeof(LoggingBehavior<,>));

        services.AddTransient(
            serviceType: typeof(IPipelineBehavior<,>),
            implementationType: typeof(PerformanceBehavior<,>));

        return services;
    }
}