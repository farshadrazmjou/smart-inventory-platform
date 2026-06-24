using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Queries;

public class GetAllProductsQueryHandler:IRequestHandler<GetAllProductsQuery,PagedResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllProductsQueryHandler> _logger;
    public GetAllProductsQueryHandler(IProductRepository repository,IMapper mapper,ILogger<GetAllProductsQueryHandler> logger)
    {
        _repository=repository;
        _mapper=mapper;
        _logger=logger;
    }

    public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery requst,CancellationToken cancellationToken)
    {
        var result=await _repository.GetAllAsync(requst.Parameter);
        _logger.LogInformation("Fetch Products complete");
        return new PagedResponse<ProductResponse>()
        {
            Items=_mapper.Map<List<ProductResponse>>(result.Items),
            Page=requst.Parameter.Page,
            PageSize=requst.Parameter.PageSize,
            TotalCount=result.TotalCount
        };
    }
}