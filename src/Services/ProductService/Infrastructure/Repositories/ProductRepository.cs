using Microsoft.EntityFrameworkCore;
using ProductService.Application.DTOs;
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
    
    public async Task<(List<Product>,int)> GetAllAsync(ProductQueryParameter parameter)
    {
        var query=_context.Products.AsQueryable();

        if(!string.IsNullOrWhiteSpace(parameter.Search))
        {
            query.Where(p => p.Name.Contains(parameter.Search));
        }

        if(!string.IsNullOrWhiteSpace(parameter.SortBy))
        {
            query=parameter.SortBy.ToLower() switch
            {
                "price" => parameter.Descending?
                                query.OrderByDescending(p => p.Price):
                                query.OrderBy(p => p.Price),
                "name" => parameter.Descending?
                                query.OrderByDescending(p => p.Name):
                                query.OrderBy(p => p.Name),
                _ => query
            };
        }

        var totalCount = await query.CountAsync();

        query=query
                .Skip((parameter.Page-1) * parameter.PageSize)
                .Take(parameter.PageSize);

        var items=await query.ToListAsync();

        return (items,totalCount);
    }
    
    public async Task<Product> AddAsync(Product product)
    {
        await _context.AddAsync(entity: product);
        await _context.SaveChangesAsync();
        return product;
    }
}