using BuildingBlocks.Exceptions.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Exceptions.Extensions;

public static class ExceptionApplicationBuilderExtensions
{
    public static WebApplication UseInventoryExceptionHandler(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        return app;
    }
}