using BuildingBlocks.Context.Interfaces;
using BuildingBlocks.Context.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Context.DependenctInjection;

public static class ContextServiceCollectionExtensions
{
    public static IServiceCollection AddRequestContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<RequestContext>();

        services.AddScoped<IRequestContext>(sp =>
            sp.GetRequiredService<RequestContext>());

        return services;
    }
}