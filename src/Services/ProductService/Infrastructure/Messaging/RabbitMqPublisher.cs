
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqSettings _settings;
    public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
    {
        _settings=options.Value;
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        var connectionFactory=new ConnectionFactory()
        {
            HostName=_settings.Host,
            UserName=_settings.UserName,
            Password=_settings.Password
        };

        var connection=await connectionFactory.CreateConnectionAsync();
        
        var channel=await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            body);
    }
}