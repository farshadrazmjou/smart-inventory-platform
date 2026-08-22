
// using System.Text.Json;
// using Microsoft.Extensions.Caching.Distributed;

// namespace ProductService.Infrastructure.Caching;

// public class RedisCacheService : IRedisCacheService
// {
//     private readonly IDistributedCache _cache;

//     public RedisCacheService(IDistributedCache cache)
//     {
//         _cache=cache;
//     }

//     public async Task AddCacheKeyAsync(string key)
//     {
//         var keys = await GetCacheKeysAsync();

//         if (!keys.Contains(key))
//         {
//             keys.Add(key);

//             await SetAsync(
//                 CacheKeys.ProductsCacheKeys,
//                 keys,
//                 TimeSpan.FromDays(30));
//         }
//     }

//     public async Task<T?> GetAsync<T>(string key)
//     {
//         var jsonValue=await _cache.GetStringAsync(key);
//         if(string.IsNullOrEmpty(jsonValue))
//             return default;
//         return JsonSerializer.Deserialize<T>(jsonValue);
//     }

//     public async Task<List<string>> GetCacheKeysAsync()
//     {
//         return await GetAsync<List<string>>(key: CacheKeys.ProductsCacheKeys) ?? new List<string>();
//     }

//     public async Task RemoveAsync(string key)
//     {
//         await _cache.RemoveAsync(key);
//     }

//     public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
//     {
//         var serializedValue=JsonSerializer.Serialize(value);

//         await _cache.SetStringAsync(
//             key,
//             value: serializedValue,
//             options: new DistributedCacheEntryOptions
//             {
//                 AbsoluteExpirationRelativeToNow=expiration
//             });
//     }

//     public async Task RemoveProductCachesAsync()
//     {
//         var keys = await GetCacheKeysAsync();
//         Console.WriteLine($"Keys Count = {keys.Count}");

//         foreach (var key in keys)
//         {
//             Console.WriteLine($"Removing Key = {key}");
//             await RemoveAsync(key);
//         }

//         await RemoveAsync(CacheKeys.ProductsCacheKeys);
//     }

// }

using System.Text.Json;
using BuildingBlocks.Observability.Activities;
using Microsoft.Extensions.Caching.Distributed;

namespace ProductService.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task AddCacheKeyAsync(string key, CancellationToken cancellationToken)
    {
        var keys = await GetCacheKeysAsync(cancellationToken);

        if (!keys.Contains(key))
        {
            keys.Add(key);

            await SetAsync(
                CacheKeys.ProductsCacheKeys,
                keys,
                TimeSpan.FromDays(30),
                cancellationToken);
        }
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var jsonValue = await _cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(jsonValue))
            return default;

        return JsonSerializer.Deserialize<T>(jsonValue);
    }

    public async Task<List<string>> GetCacheKeysAsync(CancellationToken cancellationToken)
    {
        return await GetAsync<List<string>>(CacheKeys.ProductsCacheKeys, cancellationToken) ?? new List<string>();
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken)
    {
        var serializedValue = JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(
            key,
            serializedValue,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            },
            cancellationToken);
    }

    public async Task RemoveProductCachesAsync(CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(
            InventoryActivity.Redis,
            "Invalidate Product Caches");

        try
        {
            var keys = await GetCacheKeysAsync(
                cancellationToken);

            activity?
                .SetTag("cache.keys_count", keys.Count)
                .Event("Product Cache Invalidation Started");

            foreach (var key in keys)
            {
                await RemoveAsync(
                    key,
                    cancellationToken);
            }

            await RemoveAsync(
                CacheKeys.ProductsCacheKeys,
                cancellationToken);

            activity?
                .Event("Product Caches Invalidated")
                .Success();
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(
                "request.cancelled",
                true);

            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();
            throw;
        }
    }
}