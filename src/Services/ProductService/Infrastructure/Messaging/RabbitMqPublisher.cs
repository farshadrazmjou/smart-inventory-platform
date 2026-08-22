using System.Text;
using System.Text.Json;
using BuildingBlocks.Observability.Activities;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ProductService.Infrastructure.Messaging;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqPublisher(
        IOptions<RabbitMqSettings> options,
        ILogger<RabbitMqPublisher> logger)
    {
        _settings = options.Value;
        _logger = logger;

        _connectionFactory = new ConnectionFactory
        {
            HostName = _settings.Host,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
    }

    public async Task PublishAsync<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(
            InventoryActivity.RabbitMq,
            "Publish To RabbitMQ");

        try
        {
            activity?
                .SetTag("messaging.system", "rabbitmq")
                .SetTag("messaging.destination", queueName)
                .SetTag("messaging.operation", "publish")
                .SetTag("messaging.message_type", typeof(T).Name)
                .Event("RabbitMQ Publish Started");

            await using var connection =
                await _connectionFactory.CreateConnectionAsync(
                    cancellationToken);

            await using var channel =
                await connection.CreateChannelAsync(
                    cancellationToken: cancellationToken);

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                cancellationToken: cancellationToken);

            activity?.Event("Queue Declared");

            var body = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(message));

            _logger.LogInformation(
                "Publishing {MessageType} message to RabbitMQ queue {QueueName}",
                typeof(T).Name,
                queueName);

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queueName,
                mandatory: false,
                basicProperties: new BasicProperties
                {
                    Persistent = true
                },
                body: body,
                cancellationToken: cancellationToken);

            activity?
                .Event("Message Published")
                .Success();
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag(
                "request.cancelled",
                true);

            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();

            _logger.LogError(
                ex,
                "Error publishing {MessageType} message to RabbitMQ queue {QueueName}",
                typeof(T).Name,
                queueName);

            throw;
        }
    }
}