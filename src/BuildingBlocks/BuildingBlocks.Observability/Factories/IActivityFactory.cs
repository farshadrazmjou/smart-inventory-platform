using System.Diagnostics;

namespace BuildingBlocks.Observability.Factories;

public interface IActivityFactory
{
    Activity? Start(ActivitySource source, string operationName);

    Activity? StartCurrent();
}