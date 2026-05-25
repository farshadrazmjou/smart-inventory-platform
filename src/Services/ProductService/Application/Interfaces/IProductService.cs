using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();

    Task<Product> CreateAsync(CreateProductRequest request);
}