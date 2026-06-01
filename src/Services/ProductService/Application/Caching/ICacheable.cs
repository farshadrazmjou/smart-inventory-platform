namespace ProductService.Application.Caching;

public interface ICacheable
{
    string CacheKey{get;}

    int ExpirationMinutes {get;}
}