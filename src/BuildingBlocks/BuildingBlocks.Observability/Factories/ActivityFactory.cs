using System.Diagnostics;

namespace BuildingBlocks.Observability.Factories;

public sealed class ActivityFactory : IActivityFactory
{
    public Activity? Start(ActivitySource source, string operationName)
    {
        return source.StartActivity(operationName);
    }

    public Activity? StartCurrent()
    {
        return Activity.Current;
    }
}