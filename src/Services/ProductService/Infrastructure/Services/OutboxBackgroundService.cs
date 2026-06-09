using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ProductService.Application.Events;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Messaging;

namespace ProductService.Infrastructure.Services.BackgroundServices;

public class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var outboxRepository =
                    scope.ServiceProvider
                    .GetRequiredService<IOutboxRepository>();

                var publisher =
                    scope.ServiceProvider
                    .GetRequiredService<IRabbitMqPublisher>();

                var messages =
                    await outboxRepository.GetUnprocessedAsync();

                foreach (var message in messages)
                {
                    await PublishMessage(
                        message,
                        publisher);

                    await outboxRepository
                        .MarkAsProcessedAsync(message.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox processing failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }

    private async Task PublishMessage(
        OutboxMessage message,
        IRabbitMqPublisher publisher)
    {
        if (message.Type == nameof(ProductCreatedEvent))
        {
            var productCreatedEvent =
                JsonSerializer.Deserialize<ProductCreatedEvent>(
                    message.Payload);

            if (productCreatedEvent is not null)
            {
                await publisher.PublishAsync("product-created",productCreatedEvent);
            }
        }
    }
}