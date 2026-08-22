using BuildingBlocks.Context.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Context.Extensions;

public static class ContextApplicationBuilderExtensions
{
    public static WebApplication UseInventoryRequestContext(this WebApplication app)
    {
        app.UseMiddleware<RequestContextMiddleware>();

        return app;
    }
}