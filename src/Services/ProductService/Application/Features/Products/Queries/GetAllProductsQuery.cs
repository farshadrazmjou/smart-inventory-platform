using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Queries;

public record GetAllProductsQuery(ProductQueryParameter Parameter):IRequest<PagedResponse<ProductResponse>>;