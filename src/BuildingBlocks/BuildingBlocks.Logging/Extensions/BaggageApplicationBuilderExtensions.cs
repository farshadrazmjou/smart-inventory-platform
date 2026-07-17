using BuildingBlocks.Logging.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Logging.Extensions;

public static class BaggageApplicationBuilderExtensions
{
    public static WebApplication UseInventoryBaggage(this WebApplication app)
    {
        app.UseMiddleware<BaggageMiddleware>();
        return app;
    }
}