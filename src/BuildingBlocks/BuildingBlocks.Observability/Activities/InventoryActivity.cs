using System.Diagnostics;
using BuildingBlocks.Observability.Tracing;

namespace BuildingBlocks.Observability.Activities;

public static class InventoryActivity
{
    public static readonly ActivitySource Product =
        new(ActivityNames.Product);

    public static readonly ActivitySource Auth =
        new(ActivityNames.Auth);

    public static readonly ActivitySource ApiGateway =
        new(ActivityNames.ApiGateway);

    public static readonly ActivitySource Redis =
        new(ActivityNames.Redis);

    public static readonly ActivitySource RabbitMq =
        new(ActivityNames.RabbitMq);
}