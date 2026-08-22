using System.Diagnostics;

namespace BuildingBlocks.Observability.Activities;

public static class ActivityFactory
{
    public static Activity? Start(
        ActivitySource source,
        string operationName,
        ActivityKind kind = ActivityKind.Internal)
    {
        return source.StartActivity(operationName, kind);
    }

    public static void Success(this Activity? activity)
    {
        activity?.SetStatus(ActivityStatusCode.Ok);
    }

    public static void Error(this Activity? activity,string? description = null)
    {
        activity?.SetStatus(ActivityStatusCode.Error, description);
    }

    public static void Exception(this Activity? activity, Exception exception)
    {
        if (activity == null)
            return;

        activity.AddException(exception);
        activity.SetStatus(ActivityStatusCode.Error,exception.Message);
    }

    public static Activity? Event(this Activity? activity,string eventName)
    {
        return activity?.AddEvent(new ActivityEvent(eventName));
    }
}