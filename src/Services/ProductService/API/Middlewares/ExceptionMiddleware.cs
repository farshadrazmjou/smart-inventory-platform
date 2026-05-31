using System.Net;
using System.Text.Json;
using ProductService.Application.Common;

namespace ProductService.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next=next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(FluentValidation.ValidationException ex)
        {
            context.Response.StatusCode=StatusCodes.Status400BadRequest;

            var response=new
            {
                Success=false,
                Message="Validation failed",
                Errors=ex.Errors.Select( e => e.ErrorMessage)
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch(Exception ex)
        {
            context.Response.StatusCode=(int)HttpStatusCode.InternalServerError;
            context.Response.ContentType="application/json";

            var response=new ApiResponse<string>
            {
                Success=false,
                Message=ex.Message,
                Data=null
            };

            var json=JsonSerializer.Serialize(value: response);
            await context.Response.WriteAsync(text: json);
        }
    }
}