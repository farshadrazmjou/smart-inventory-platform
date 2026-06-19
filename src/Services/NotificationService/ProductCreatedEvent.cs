namespace NotificationService;

public class ProductCreatedEvent
{
    public Guid Id{get;set;}
    public string Name{get;set;}=string.Empty;
    public float Price{get;set;}
}