namespace ProductService.Infrastructure.Caching;

public interface IRedisCacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan expiration);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
    Task RemoveProductCachesAsync();
    Task AddCacheKeyAsync(string key);
    Task<List<string>> GetCacheKeysAsync();
}