using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    Task<(List<Product> Items ,int TotalCount)> GetAllAsync(ProductQueryParameter parameter);

    Task<Product> AddAsync(Product product);
}