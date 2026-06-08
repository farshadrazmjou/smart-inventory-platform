namespace ProductService.Infrastructure.Messaging;

public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}