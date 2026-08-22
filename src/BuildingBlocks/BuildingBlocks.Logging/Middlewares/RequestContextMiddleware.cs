// using System.Security.Claims;
// using BuildingBlocks.Context.Interfaces;
// using Microsoft.AspNetCore.Http;

// namespace BuildingBlocks.Context.Middlewares;

// public sealed class RequestContextMiddleware
// {
//     private readonly RequestDelegate _next;

//     public RequestContextMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }

//     public async Task InvokeAsync(HttpContext context, IRequestContext requestContext)
//     {
//         requestContext.CorrelationId = context.TraceIdentifier;

//         requestContext.User.UserId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

//         requestContext.User.Username = context.User.Identity?.Name;

//         requestContext.ClientIp = context.Connection.RemoteIpAddress?.ToString();

//         requestContext.UserAgent = context.Request.Headers.UserAgent.ToString();

//         await _next(context);
//     }
// }