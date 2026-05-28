using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryParameter productQueryParameter);

    Task<ProductResponse> CreateAsync(CreateProductRequest request);
}