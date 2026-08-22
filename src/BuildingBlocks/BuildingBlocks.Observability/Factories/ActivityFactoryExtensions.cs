// using System.Diagnostics;
// using BuildingBlocks.Observability.Tracing;

// namespace BuildingBlocks.Observability.Factories;

// public static class ActivityFactoryExtensions
// {
//     public static Activity? StartAuth(this IActivityFactory factory)
//     {
//         return factory.Start(ActivityNames.Auth,kind: ActivityKind.Internal);
//         //return factory.StartCurrent();
//     }

//     public static Activity? StartProduct(this IActivityFactory factory)
//     {
//         // return factory.Start(ActivityNames.Product);
//         return factory.StartCurrent();
//     }

//     public static Activity? StartApiGateway(this IActivityFactory factory)
//     {
//         // return factory.Start(ActivityNames.ApiGateway);
//         return factory.StartCurrent();
//     }
// }

using System.Diagnostics;

namespace BuildingBlocks.Observability.Activities;

public static class ActivityExtensions
{
    public static Activity? UserId(
        this Activity? activity,
        Guid userId)
    {
        activity?.SetTag("user.id", userId);

        return activity;
    }

    public static Activity? Username(
        this Activity? activity,
        string username)
    {
        activity?.SetTag("user.username", username);

        return activity;
    }

    public static Activity? LoginResult(
        this Activity? activity,
        string result)
    {
        activity?.SetTag("login.result", result);

        return activity;
    }

    public static Activity? ClientIp(
        this Activity? activity,
        string? ip)
    {
        activity?.SetTag("client.ip", ip);

        return activity;
    }

    public static Activity? JwtIssued(
        this Activity? activity)
    {
        activity?.AddEvent(
            new ActivityEvent("JWT Generated"));

        return activity;
    }
}