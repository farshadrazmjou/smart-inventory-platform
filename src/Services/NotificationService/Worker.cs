using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly RabbitMqSettings _settings;

    public Worker(ILogger<Worker> logger , IOptions<RabbitMqSettings> options)
    {
        _logger = logger;
        _settings=options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationService Started");

        var factory=new ConnectionFactory()
        {
            HostName = _settings.Host,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        var connection=await factory.CreateConnectionAsync(stoppingToken);
        var channel=await connection.CreateChannelAsync();
        await channel.QueueDeclareAsync(
            queue: "product-created",
            durable: true,
            exclusive: false,
            autoDelete: false );

        var consumer=new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (_,args) =>
        {
            Console.WriteLine("Message received");

            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var product = JsonSerializer.Deserialize<ProductCreatedEvent>(json);

            _logger.LogInformation(
                "NotificationService => Product Created: Id={Id} Name={Name} Price={Price}",
                product?.Id,
                product?.Name,
                product?.Price);

            await channel.BasicAckAsync(args.DeliveryTag,false);

            Console.WriteLine(json);
        };

        await channel.BasicConsumeAsync(
            queue: "product-created",
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite,stoppingToken);
    }
}
