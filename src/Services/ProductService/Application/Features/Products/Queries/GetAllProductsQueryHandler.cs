using AutoMapper;
using Azure.Core;
using BuildingBlocks.Observability.Activities;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Queries;

// public class GetAllProductsQueryHandler:IRequestHandler<GetAllProductsQuery,PagedResponse<ProductResponse>>
// {
//     private readonly IProductRepository _repository;
//     private readonly IMapper _mapper;
//     private readonly ILogger<GetAllProductsQueryHandler> _logger;
//     public GetAllProductsQueryHandler(IProductRepository repository,IMapper mapper,ILogger<GetAllProductsQueryHandler> logger)
//     {
//         _repository=repository;
//         _mapper=mapper;
//         _logger=logger;
//     }

//     public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery request,CancellationToken cancellationToken)
//     {
//         _logger.LogInformation("===== HANDLER START =====");
//         var activity=InventoryActivity.Product.StartActivity("GetAllProducts");
//         Console.WriteLine(activity == null ? "Activity NULL" : "Activity CREATED");
//         _logger.LogInformation(activity == null ? "Activity NULL"  : "Activity CREATED");

//         activity?.SetTag("page",request.Parameter.Page);
//         activity?.SetTag("page.size",request.Parameter.PageSize);

//         _logger.LogInformation("Getting products...");

//         var result=await _repository.GetAllAsync(request.Parameter);
        
//         activity?.AddEvent(new(name: "Products fetched from repository"));

//         var response= new PagedResponse<ProductResponse>()
//         {
//             Items=_mapper.Map<List<ProductResponse>>(result.Items),
//             Page=request.Parameter.Page,
//             PageSize=request.Parameter.PageSize,
//             TotalCount=result.TotalCount
//         };

//         activity?.SetTag("products.count", response.Items.Count);

//         return response;
//     }
// }

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResponse<ProductResponse>>
{
    private readonly IProductService _productService;

    public GetAllProductsQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<PagedResponse<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        return await _productService.GetAllProductsAsync(request.Parameter, cancellationToken);
    }
}