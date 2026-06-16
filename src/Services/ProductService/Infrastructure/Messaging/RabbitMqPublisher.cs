
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly ConnectionFactory _connectionFactory;
    
    public RabbitMqPublisher(IOptions<RabbitMqSettings> options,ILogger<RabbitMqPublisher> logger)
    {
        _settings=options.Value;
        _logger=logger;
        _connectionFactory=new ConnectionFactory()
        {
            HostName=_settings.Host,
            UserName=_settings.UserName,
            Password=_settings.Password
        };
    }

    public async Task PublishAsync<T>(string queueName, T message)
    {
        await using var connection=await _connectionFactory.CreateConnectionAsync();
        
        await using var channel=await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        _logger.LogInformation($"Publishing message to queue {queueName}");

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            basicProperties: new BasicProperties { Persistent = true },
            body: body);
    }
}