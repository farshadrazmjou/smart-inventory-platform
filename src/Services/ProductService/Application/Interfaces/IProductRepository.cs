using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    Task<(List<Product> Items ,int TotalCount)> GetAllAsync(ProductQueryParameter parameter);

    Task<Product> GetByIdAsync(Guid Id);

    Task<Product> AddAsync(Product product);

    Task<Product> UpdateAsync(Guid Id,Product product);

    Task<Product> DeleteAsync(Guid Id);
}