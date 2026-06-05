
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace ProductService.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache=cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonValue=await _cache.GetStringAsync(key);
        if(string.IsNullOrEmpty(jsonValue))
            return default;
        return JsonSerializer.Deserialize<T>(jsonValue);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        var serializedValue=JsonSerializer.Serialize(value);

        await _cache.SetStringAsync(
            key,
            value: serializedValue,
            options: new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow=expiration
            });
    }
}