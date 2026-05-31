using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Commands;

public record CreateProductCommand(CreateProductRequest CreateProductRequest):
                                                            IRequest<ApiResponse<ProductResponse>>;