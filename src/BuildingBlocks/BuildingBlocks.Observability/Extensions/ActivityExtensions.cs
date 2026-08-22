using System.Diagnostics;
using BuildingBlocks.Observability.Tags;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability.Extensions;

public static class ActivityExtensions
{
    public static Activity? SetUser(this Activity? activity, string? userId, string? username, string? role = null)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.UserId, userId);
        activity.SetTag(TagNames.Username, username);

        if (!string.IsNullOrWhiteSpace(role))
            activity.SetTag(TagNames.UserRole, role);

        return activity;
    }

    public static Activity? SetCorrelationId(this Activity? activity, string? correlationId)
    {
        activity?.SetTag(TagNames.CorrelationId, correlationId);
        return activity;
    }

    public static Activity? SetClientIp(this Activity? activity, string? ip)
    {
        activity?.SetTag(TagNames.ClientIp, ip);
        return activity;
    }

    public static Activity? SetEntity(this Activity? activity, string entityName, object entityId)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.EntityName, entityName);
        activity.SetTag(TagNames.EntityId, entityId);

        return activity;
    }

    public static Activity? SetCacheHit(this Activity? activity, string key)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.CacheHit, true);
        activity.SetTag(TagNames.CacheKey, key);

        activity.AddEvent(new ActivityEvent(EventNames.CacheHit));

        return activity;
    }

    public static Activity? SetCacheMiss(this Activity? activity, string key)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.CacheHit, false);
        activity.SetTag(TagNames.CacheKey, key);

        activity.AddEvent(new ActivityEvent(EventNames.CacheMiss));

        return activity;
    }

    public static Activity? LoginSucceeded(this Activity? activity)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.LoginResult, "success");
        activity.AddEvent(new ActivityEvent(EventNames.UserAuthenticated));
        activity.SetStatus(ActivityStatusCode.Ok);

        return activity;
    }

    public static Activity? LoginFailed(this Activity? activity, string reason)
    {
        if (activity is null)
            return activity;

        activity.SetTag(TagNames.LoginResult, reason);
        activity.SetStatus(ActivityStatusCode.Error);

        return activity;
    }

    public static Activity? RecordError(this Activity? activity, Exception ex)
    {
        if (activity is null)
            return activity;

        activity.AddException(ex);
        activity.SetStatus(ActivityStatusCode.Error, ex.Message);

        return activity;
    }

    public static Activity? AddBusinessEvent(this Activity? activity, string eventName)
    {
        activity?.AddEvent(new ActivityEvent(eventName));

        return activity;
    }
}