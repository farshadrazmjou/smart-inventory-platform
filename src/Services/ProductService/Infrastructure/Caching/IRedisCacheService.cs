namespace ProductService.Infrastructure.Caching;

public interface IRedisCacheService
{
    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken);

    Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken);

    Task RemoveAsync(
        string key,
        CancellationToken cancellationToken);

    Task RemoveProductCachesAsync(
        CancellationToken cancellationToken);

    Task AddCacheKeyAsync(
        string key,
        CancellationToken cancellationToken);

    Task<List<string>> GetCacheKeysAsync(
        CancellationToken cancellationToken);
}