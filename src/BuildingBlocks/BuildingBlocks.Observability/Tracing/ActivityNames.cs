namespace BuildingBlocks.Observability.Tracing;

public static class ActivityNames
{
    public const string Auth = "AuthService.Business";
    public const string Product = "ProductService.Business";
    public const string Order = "OrderService.Business";
    public const string Inventory = "InventoryService.Business";
    public const string Notification = "NotificationService.Business";
    public const string ApiGateway = "ApiGateway.Business";    
    public const string Redis = "Inventory.Redis";
    public const string RabbitMq = "Inventory.RabbitMQ";
}