using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository:IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context=context;
    }
    
    public async Task<List<Product>> GetAllAsync()
        => await _context.Products.ToListAsync();
    
    public async Task<Product> AddAsync(Product product)
    {
        await _context.AddAsync(entity: product);
        await _context.SaveChangesAsync();
        return product;
    }
}