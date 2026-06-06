using MediatR;
using Microsoft.Extensions.Caching.Memory;
using ProductService.Application.Caching;
using ProductService.Infrastructure.Caching;

namespace ProductService.Application.Behaviors;

public class CachingBehavior<TRequest, TResponse> :
                IPipelineBehavior<TRequest, TResponse>
                where TRequest : IRequest<TResponse>
{
    private readonly IRedisCacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest,TResponse>> _logger;
    public CachingBehavior(IRedisCacheService cache,ILogger<CachingBehavior<TRequest,TResponse>> logger)
    {
        _cache=cache;
        _logger=logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        
        if (request is not ICacheable cacheable)
        {
            return await next();
        }

        var cacheResponse=await _cache.GetAsync<TResponse>(cacheable.CacheKey);
        if (cacheResponse is not null)
        {
            return cacheResponse;
        }

        var response = await next();

        await _cache.SetAsync(
            key: cacheable.CacheKey,
            value: response,
            expiration: TimeSpan.FromMinutes(cacheable.ExpirationMinutes));

        try
        {
            if (cacheable.CacheKey.StartsWith(CacheKeys.ProductsPrefix))
            {
                await _cache.AddCacheKeyAsync(
                    cacheable.CacheKey);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError($"Error on caching {ex.Message}");
        }

        return response;
    }

}