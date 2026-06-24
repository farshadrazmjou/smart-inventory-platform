using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Queries;

public record GetProductByIdQuery(Guid ProductId) : IRequest<ApiResponse<ProductResponse>>;
