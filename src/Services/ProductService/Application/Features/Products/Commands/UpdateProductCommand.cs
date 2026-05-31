using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products;

public record UpdateProductCommand(Guid Id,UpdateProductRequest Request) : 
                    IRequest<ApiResponse<ProductResponse>>;