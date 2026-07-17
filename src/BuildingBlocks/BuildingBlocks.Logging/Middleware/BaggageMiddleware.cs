// using System.Diagnostics;
// using Microsoft.AspNetCore.Http;
// using OpenTelemetry;

// namespace BuildingBlocks.Logging.Middleware;

// public sealed class BaggageMiddleware
// {
//     private readonly RequestDelegate _next;

//     public BaggageMiddleware(RequestDelegate next)
//     {
//         _next = next;
//     }

//     public async Task Invoke(HttpContext context)
//     {
//         var user = context.User;

//         var correlationId = context.TraceIdentifier;
//         Baggage.SetBaggage(name: "correlation.id", value: correlationId);

//         if (user.Identity?.IsAuthenticated == true)
//         {
//             var userId = user.FindFirst("sub")?.Value;
//             var username = user.Identity.Name;
//             var role = user.FindFirst("role")?.Value;
            
//             if (!string.IsNullOrWhiteSpace(userId))
//                 Baggage.SetBaggage("user.id", userId);

//             if (!string.IsNullOrWhiteSpace(username))
//                 Baggage.SetBaggage("user.name", username);

//             if (!string.IsNullOrWhiteSpace(role))
//                 Baggage.SetBaggage("user.role", role);
//         }

//         await _next(context);
//     }
// }

using System.Diagnostics;
using BuildingBlocks.Logging.Interfaces;
using Microsoft.AspNetCore.Http;
using OpenTelemetry;

namespace BuildingBlocks.Logging.Middleware;

public sealed class BaggageMiddleware
{
    private readonly RequestDelegate _next;

    public BaggageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserContextAccessor userContext)
    {
        // CorrelationId
        Baggage.SetBaggage("CorrelationId", context.TraceIdentifier);

        // User Information
        if (userContext.IsAuthenticated)
        {
            if (!string.IsNullOrWhiteSpace(userContext.UserId))
            {
                Baggage.SetBaggage("UserId", userContext.UserId);
            }

            if (!string.IsNullOrWhiteSpace(userContext.Username))
            {
                Baggage.SetBaggage("Username", userContext.Username);
            }

            if (!string.IsNullOrWhiteSpace(userContext.Role))
            {
                Baggage.SetBaggage("Role", userContext.Role);
            }
        }

        await _next(context);
    }
}