using System.Diagnostics;
using BuildingBlocks.Observability.Activities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.MediatR.Behaviors;

public class TracingBehavior<TRequest, TResponse> :
                            IPipelineBehavior<TRequest, TResponse>
                                where TRequest : IRequest<TResponse>
{
    private readonly ILogger<TracingBehavior<TRequest, TResponse>> _logger;

    public TracingBehavior(ILogger<TracingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
                    TRequest request
                    , RequestHandlerDelegate<TResponse> next
                    , CancellationToken cancellationToken)
    {
        using var activity = InventoryActivity.Product.StartActivity(
                typeof(TRequest).Name,
                ActivityKind.Internal);

        activity?.SetTag("request.type", typeof(TRequest).FullName);

        _logger.LogInformation("Handling request {RequestName}", typeof(TRequest).Name);

        var response = await next();

        activity?.SetStatus(ActivityStatusCode.Ok);

        return response;
    }
}