using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductService _productService;

    public UpdateProductCommandHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(UpdateProductCommand request,CancellationToken cancellationToken)
    {
        return await _productService.UpdateProductAsync(request.Id,request.Request,cancellationToken);
    }
}