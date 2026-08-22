using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message,CancellationToken cancellationToken);

    Task<List<OutboxMessage>> GetUnprocessedAsync(CancellationToken cancellationToken);

    Task MarkAsProcessedAsync(Guid id,CancellationToken cancellationToken);

    Task MarkAsFailedAttemptAsync(
        Guid id,
        int retryCount,
        DateTime lastAttemptAt,
        string error,
        CancellationToken cancellationToken);
}