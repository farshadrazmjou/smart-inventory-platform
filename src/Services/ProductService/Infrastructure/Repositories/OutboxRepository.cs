using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly ProductDbContext _context;

    public OutboxRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await _context.OutboxMessages.AddAsync(message, cancellationToken);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OutboxMessage>> GetUnprocessedAsync(CancellationToken cancellationToken)
    {
        return await _context.OutboxMessages
            .AsNoTracking()
            .Where(x => !x.Processed)
            .OrderBy(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsProcessedAsync(Guid id, CancellationToken cancellationToken)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (message is null)
            return;

        message.Processed = true;
        message.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    

    public async Task MarkAsFailedAttemptAsync(
    Guid id,
    int retryCount,
    DateTime lastAttemptAt,
    string error,
    CancellationToken cancellationToken)
    {
        var message = await _context.OutboxMessages
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (message is null)
            return;

        message.RetryCount = retryCount;
        message.LastAttemptAt = lastAttemptAt;
        message.LastError = error;

        await _context.SaveChangesAsync(
            cancellationToken);
    }

}
