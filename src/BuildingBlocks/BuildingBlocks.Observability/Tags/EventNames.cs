namespace BuildingBlocks.Observability.Tags;

public static class EventNames
{
    public const string CacheHit = "Cache Hit";

    public const string CacheMiss = "Cache Miss";

    public const string UserAuthenticated = "User Authenticated";

    public const string JwtGenerated = "JWT Generated";

    public const string ProductCreated = "Product Created";

    public const string ProductUpdated = "Product Updated";

    public const string RabbitPublished = "RabbitMQ Published";

    public const string RedisSaved = "Redis Saved";
}