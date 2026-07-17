using System.Diagnostics;
using System.Text.Json;
using BuildingBlocks.Context.Interfaces;
using BuildingBlocks.Exceptions.Exceptions;
using BuildingBlocks.Exceptions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Exceptions.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception. TraceId:{TraceId} CorrelationId:{CorrelationId}",
                Activity.Current?.TraceId.ToString(),
                requestContext.CorrelationId);

            await WriteResponseAsync(context, exception: ex, requestContext);
        }
    }

    private static async Task WriteResponseAsync(
        HttpContext context,
        Exception exception,
        IRequestContext requestContext)
    {
        var statusCode = exception switch
        {
            BusinessException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        var response = new ApiErrorResponse
        {
            StatusCode = statusCode,
            Title = exception.GetType().Name,
            Detail = exception.Message,
            TraceId = Activity.Current?.TraceId.ToString() ?? "",
            CorrelationId = requestContext.CorrelationId
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}