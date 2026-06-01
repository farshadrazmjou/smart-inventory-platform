using MediatR;
using Microsoft.Extensions.Caching.Memory;
using ProductService.Application.Caching;

namespace ProductService.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> :
                IPipelineBehavior<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;

    public CachingBehavior(IMemoryCache cache)
    {
        _cache=cache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        
        if (request is not ICacheable cacheable)
        {
            return await next();
        }

        if (_cache.TryGetValue(key: cacheable.CacheKey, value: out TResponse? cachedResponse))
        {
            return cachedResponse!;
        }

        var response = await next();

        _cache.Set(
            key: cacheable.CacheKey,
            value: response,
            absoluteExpirationRelativeToNow: TimeSpan.FromMinutes(cacheable.ExpirationMinutes));

        return response;
    }

}