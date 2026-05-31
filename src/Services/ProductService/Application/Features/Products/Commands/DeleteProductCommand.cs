using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Commands;

public record DeleteProductCommand(Guid Id):IRequest<ApiResponse<ProductResponse>>;