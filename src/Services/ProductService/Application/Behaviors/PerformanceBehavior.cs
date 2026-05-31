using System.Diagnostics;
using Azure.Core;
using MediatR;

namespace ProductService.Application.Behaviors;

public class PerformanceBehavior<TRequest, TResponse> :
                    IPipelineBehavior<TRequest, TResponse>
                    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();

        var elapsed = stopwatch.ElapsedMilliseconds;


        if (elapsed > 500)        
            _logger.LogWarning($"SLOW REQUEST ⚠ {requestName} took {elapsed} ms");        
        else        
            _logger.LogInformation( $"Request {requestName} executed in {elapsed} ms");
        
        return response;
    }
}