using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductService.Infrastructure.Messaging;

public class ProductCreatedConsumer : BackgroundService
{
    private readonly ILogger<ProductCreatedConsumer> _logger;
    private readonly RabbitMqSettings _settings;

    public ProductCreatedConsumer(ILogger<ProductCreatedConsumer> logger,IOptions<RabbitMqSettings> settings)
    {
        _logger=logger;
        _settings=settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        var connection = await factory.CreateConnectionAsync(cancellationToken: stoppingToken);

        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: "product-created",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (sender , ea) =>
        {
            var body=ea.Body.ToArray();
            var message=Encoding.UTF8.GetString(body);
            _logger.LogInformation(message: $"RabbitMQ Message Received: {message}");
            await Task.CompletedTask;
        };

         await channel.BasicConsumeAsync(
            queue: "product-created",
            autoAck: true,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(
            millisecondsDelay: Timeout.Infinite,
            cancellationToken: stoppingToken);
    }
}