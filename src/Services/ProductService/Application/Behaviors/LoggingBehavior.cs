using System.Diagnostics;
using MediatR;

namespace ProductService.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse> :
                IPipelineBehavior<TRequest, TResponse>
                    where TRequest : IRequest<TResponse>

{
    private readonly ILogger<LoggingBehavior<TRequest,TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest,TResponse>> logger)
    {
        _logger=logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName=typeof(TRequest).Name;
        
        _logger.LogInformation(message: $"Handling request {requestName}");

        var stopWatch=Stopwatch.StartNew();
        var response = await next();
        stopWatch.Stop();

        _logger.LogInformation(message: $"Handled request {requestName} in {stopWatch.ElapsedMilliseconds} ms");

        return response;
    }
}