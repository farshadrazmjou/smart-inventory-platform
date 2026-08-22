using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductService _productService;

    public DeleteProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        return await _productService.DeleteProductAsync(request.Id, cancellationToken);
    }
}