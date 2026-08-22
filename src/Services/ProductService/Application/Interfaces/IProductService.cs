using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductResponse>> GetAllProductsAsync(
        ProductQueryParameter parameter,
        CancellationToken cancellationToken);

    Task<ApiResponse<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken);

    Task<ApiResponse<ProductResponse>> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<ApiResponse<ProductResponse>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken);

    Task<ApiResponse<ProductResponse>> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken);
}