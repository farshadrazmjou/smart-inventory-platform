using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    Task<(List<Product> Items ,int TotalCount)> GetAllAsync(ProductQueryParameter parameter,CancellationToken cancellationToken);

    Task<Product> GetByIdAsync(Guid Id,CancellationToken cancellationToken);

    Task<Product> AddAsync(Product product,CancellationToken cancellationToken);

    Task<Product> UpdateAsync(Guid Id,Product product,CancellationToken cancellationToken);

    Task<Product?> DeleteAsync(Guid id,CancellationToken cancellationToken);
}