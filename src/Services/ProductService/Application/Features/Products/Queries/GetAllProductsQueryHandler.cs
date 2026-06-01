using System.Reflection.Metadata;
using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Queries;

public class GetAllProductsQueryHandler:IRequestHandler<GetAllProductsQuery,PagedResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetAllProductsQueryHandler(IProductRepository repository,IMapper mapper)
    {
        _repository=repository;
        _mapper=mapper;
    }

    public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery requst,CancellationToken cancellationToken)
    {
        await Task.Delay(6000);

        var result=await _repository.GetAllAsync(requst.Parameter);
        return new PagedResponse<ProductResponse>()
        {
            Items=_mapper.Map<List<ProductResponse>>(result.Items),
            Page=requst.Parameter.Page,
            PageSize=requst.Parameter.PageSize,
            TotalCount=result.TotalCount
        };
    }

}