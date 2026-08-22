using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Exceptions.DependencyInjection;

public static class ExceptionServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryExceptionHandling(this IServiceCollection services)
    {
        return services;
    }
}