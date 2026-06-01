using MediatR;
using ProductService.Application.Caching;
using ProductService.Application.Common;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Queries;

public record GetAllProductsQuery(ProductQueryParameter Parameter) :
                    IRequest<PagedResponse<ProductResponse>>, ICacheable
{
    public string CacheKey => $"products-{Parameter.Page}-{Parameter.PageSize}";

    public int ExpirationMinutes => 5;
}