// // using System.Text.Json;
// // using ProductService.Application.Events;
// // using ProductService.Application.Interfaces;
// // using ProductService.Domain.Entities;
// // using ProductService.Infrastructure.Messaging;

// // namespace ProductService.Infrastructure.Services.BackgroundServices;

// // public class OutboxBackgroundService : BackgroundService
// // {
// //     private readonly IServiceScopeFactory _scopeFactory;
// //     private readonly ILogger<OutboxBackgroundService> _logger;

// //     public OutboxBackgroundService(
// //         IServiceScopeFactory scopeFactory,
// //         ILogger<OutboxBackgroundService> logger)
// //     {
// //         _scopeFactory = scopeFactory;
// //         _logger = logger;
// //     }

// //     protected override async Task ExecuteAsync(CancellationToken cancellationToken)
// //     {
// //         while (!cancellationToken.IsCancellationRequested)
// //         {
// //             try
// //             {
// //                 using var scope =
// //                     _scopeFactory.CreateScope();

// //                 var outboxRepository =
// //                     scope.ServiceProvider
// //                     .GetRequiredService<IOutboxRepository>();

// //                 var publisher =
// //                     scope.ServiceProvider
// //                     .GetRequiredService<IRabbitMqPublisher>();

// //                 var messages =
// //                     await outboxRepository.GetUnprocessedAsync(cancellationToken);

// //                 foreach (var message in messages)
// //                 {
// //                     await PublishMessage(message,publisher, cancellationToken);
// //                     await outboxRepository.MarkAsProcessedAsync(message.Id,cancellationToken);
// //                 }
// //             }
// //             catch (Exception ex)
// //             {
// //                 _logger.LogError(ex,"Outbox processing failed");
// //             }

// //             await Task.Delay(TimeSpan.FromSeconds(5),cancellationToken);
// //         }
// //     }

// //     private async Task PublishMessage(OutboxMessage message, IRabbitMqPublisher publisher,CancellationToken cancellationToken)
// //     {
// //         if (message.Type == nameof(ProductCreatedEvent))
// //         {
// //             var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(message.Payload);

// //             if (productCreatedEvent is not null)
// //             {
// //                 await publisher.PublishAsync("product-created",productCreatedEvent,cancellationToken:cancellationToken);
// //             }
// //         }
// //     }
// // }

// using System.Text.Json;
// using BuildingBlocks.Observability.Activities;
// using ProductService.Application.Events;
// using ProductService.Application.Interfaces;
// using ProductService.Domain.Entities;
// using ProductService.Infrastructure.Messaging;

// namespace ProductService.Infrastructure.Services.BackgroundServices;

// public class OutboxBackgroundService : BackgroundService
// {
//     private readonly IServiceScopeFactory _scopeFactory;
//     private readonly ILogger<OutboxBackgroundService> _logger;

//     public OutboxBackgroundService(
//         IServiceScopeFactory scopeFactory,
//         ILogger<OutboxBackgroundService> logger)
//     {
//         _scopeFactory = scopeFactory;
//         _logger = logger;
//     }

//     protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//     {
//         _logger.LogInformation("Outbox Background Service started");

//         while (!cancellationToken.IsCancellationRequested)
//         {
//             try
//             {
//                 using var scope = _scopeFactory.CreateScope();

//                 var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
//                 var publisher =scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

//                 var messages =await outboxRepository.GetUnprocessedAsync(cancellationToken);

//                 foreach (var message in messages)
//                 {
//                     await ProcessMessageAsync(
//                         message,
//                         outboxRepository,
//                         publisher,
//                         cancellationToken);
//                 }
//             }
//             catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
//             {
//                 break;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex,"Outbox processing failed");
//             }

//             try
//             {
//                 await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
//             }
//             catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
//             {
//                 break;
//             }
//         }

//         _logger.LogInformation("Outbox Background Service stopped");
//     }

//     private async Task ProcessMessageAsync(
//         OutboxMessage message,
//         IOutboxRepository outboxRepository,
//         IRabbitMqPublisher publisher,
//         CancellationToken cancellationToken)
//     {
//         using var activity =
//             ActivityFactory.Start(InventoryActivity.Product,"Process Outbox Message");

//         try
//         {
//             activity?
//                 .SetTag("outbox.message_id", message.Id)
//                 .SetTag("outbox.message_type", message.Type)
//                 .SetTag("outbox.processed", message.Processed)
//                 .Event("Outbox Processing Started");

//             if (message.Type == nameof(ProductCreatedEvent))
//             {
//                 var productCreatedEvent =
//                     JsonSerializer.Deserialize<ProductCreatedEvent>(
//                         message.Payload);

//                 if (productCreatedEvent is null)
//                 {
//                     throw new InvalidOperationException($"Unable to deserialize outbox message {message.Id}");
//                 }

//                 activity?
//                     .SetTag(
//                         "product.id",
//                         productCreatedEvent.Id)
//                     .SetTag(
//                         "messaging.destination",
//                         "product-created");

//                 await publisher.PublishAsync(
//                     "product-created",
//                     productCreatedEvent,
//                     cancellationToken);

//                 activity?.Event(
//                     "ProductCreatedEvent Published");
//             }
//             else
//             {
//                 _logger.LogWarning(
//                     "Unknown outbox message type {MessageType} for message {MessageId}",
//                     message.Type,
//                     message.Id);

//                 activity?
//                     .SetTag(
//                         "outbox.unknown_type",
//                         true)
//                     .Event("Unknown Outbox Message Type");

//                 return;
//             }

//             // فقط بعد از Publish موفق
//             await outboxRepository.MarkAsProcessedAsync(message.Id,cancellationToken);

//             activity?
//                 .SetTag("outbox.processed", true)
//                 .Event("Outbox Message Marked As Processed")
//                 .Success();

//             _logger.LogInformation(
//                 "Outbox message {MessageId} processed successfully",
//                 message.Id);
//         }
//         catch (OperationCanceledException)
//             when (cancellationToken.IsCancellationRequested)
//         {
//             activity?.SetTag("request.cancelled",true);
//             throw;
//         }
//         catch (Exception ex)
//         {
//             activity?.AddException(ex).Error();

//             _logger.LogError(
//                 ex,
//                 "Failed to process outbox message {MessageId}",
//                 message.Id);

//             // عمداً MarkAsProcessed نمی‌کنیم
//             // تا Worker در اجرای بعدی دوباره تلاش کند.

//             throw;
//         }
//     }
// }

using System.Text.Json;
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
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var outboxRepository =
                    scope.ServiceProvider
                        .GetRequiredService<IOutboxRepository>();

                var publisher =
                    scope.ServiceProvider
                        .GetRequiredService<IRabbitMqPublisher>();

                var messages =
                    await outboxRepository.GetUnprocessedAsync(
                        cancellationToken);

                foreach (var message in messages)
                {
                    await ProcessMessageAsync(
                        message,
                        outboxRepository,
                        publisher,
                        cancellationToken);
                }
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox processing failed");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                cancellationToken);
        }
    }

    // private async Task ProcessMessageAsync(
    //     OutboxMessage message,
    //     IOutboxRepository outboxRepository,
    //     IRabbitMqPublisher publisher,
    //     CancellationToken cancellationToken)
    // {
    //     try
    //     {
    //         message.RetryCount++;
    //         message.LastAttemptAt = DateTime.UtcNow;

    //         _logger.LogInformation(
    //             "Processing Outbox Message {MessageId}. Attempt: {RetryCount}",
    //             message.Id,
    //             message.RetryCount);

    //         await PublishMessage(
    //             message,
    //             publisher,
    //             cancellationToken);

    //         await outboxRepository.MarkAsProcessedAsync(
    //             message.Id,
    //             cancellationToken);

    //         _logger.LogInformation(
    //             "Outbox Message {MessageId} processed successfully",
    //             message.Id);
    //     }
    //     catch (Exception ex)
    //     {
    //         message.LastError = ex.Message;

    //         _logger.LogError(
    //             ex,
    //             "Failed to process Outbox Message {MessageId}. Attempt: {RetryCount}",
    //             message.Id,
    //             message.RetryCount);
    //     }
    // }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        IOutboxRepository outboxRepository,
        IRabbitMqPublisher publisher,
        CancellationToken cancellationToken)
    {
        var retryCount = message.RetryCount + 1;
        var attemptTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation(
                "Processing Outbox Message {MessageId}. Attempt: {RetryCount}",
                message.Id,
                retryCount);

            await PublishMessage(
                message,
                publisher,
                cancellationToken);

            await outboxRepository.MarkAsProcessedAsync(
                message.Id,
                cancellationToken);

            _logger.LogInformation(
                "Outbox Message {MessageId} processed successfully",
                message.Id);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await outboxRepository.MarkAsFailedAttemptAsync(
                message.Id,
                retryCount,
                attemptTime,
                ex.Message,
                cancellationToken);

            _logger.LogError(
                ex,
                "Failed to process Outbox Message {MessageId}. Attempt: {RetryCount}",
                message.Id,
                retryCount);
        }
    }

    private async Task PublishMessage(
        OutboxMessage message,
        IRabbitMqPublisher publisher,
        CancellationToken cancellationToken)
    {
        if (message.Type == nameof(ProductCreatedEvent))
        {
            var productCreatedEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(message.Payload);

            if (productCreatedEvent is null)
            {
                throw new InvalidOperationException($"Unable to deserialize Outbox Message {message.Id}");
            }

            await publisher.PublishAsync("product-created", productCreatedEvent, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Unknown Outbox Message Type: {message.Type}");
        }
    }
}