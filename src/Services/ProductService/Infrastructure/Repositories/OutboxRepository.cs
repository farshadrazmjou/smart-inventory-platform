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
    
    public async Task AddAsync(OutboxMessage message)
    {
        await _context.OutboxMessages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OutboxMessage>> GetUnprocessedAsync()
    {
        return await _context.OutboxMessages
            .Where(x => !x.Processed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task MarkAsProcessedAsync(Guid id)
    {
        var message =
            await _context.OutboxMessages.FindAsync(id);

        if (message is null)
            return;

        message.Processed = true;

        await _context.SaveChangesAsync();
    }
}