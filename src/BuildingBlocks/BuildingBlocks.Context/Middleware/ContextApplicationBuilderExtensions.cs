using BuildingBlocks.Context.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Context.Extensions;

public static class ContextApplicationBuilderExtensions
{
    public static WebApplication UseRequestContext(this WebApplication app)
    {
        app.UseMiddleware<RequestContextMiddleware>();

        return app;
    }
}