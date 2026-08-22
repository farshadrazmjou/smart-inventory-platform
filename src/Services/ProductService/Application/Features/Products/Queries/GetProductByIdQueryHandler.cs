// using AutoMapper;
// using MediatR;
// using ProductService.Application.Common;
// using ProductService.Application.DTOs;
// using ProductService.Application.Interfaces;

// namespace ProductService.Application.Features.Products.Queries;

// public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery,ApiResponse<ProductResponse>>
// {
//     private readonly IProductRepository _productRepository;
//     private readonly IMapper _mapper;
//     private readonly ILogger<GetProductByIdQueryHandler> _logger;
//     public GetProductByIdQueryHandler(IProductRepository productRepository,IMapper mapper,ILogger<GetProductByIdQueryHandler> logger)
//     {
//         _productRepository=productRepository;
//         _mapper=mapper;
//         _logger=logger;
//     }

//     public async Task<ApiResponse<ProductResponse>> Handle(GetProductByIdQuery query,CancellationToken token)
//     {
//         var product=await _productRepository.GetByIdAsync(query.ProductId);
//         _logger.LogInformation("Fetch product successfully");
        
//         var response=new ApiResponse<ProductResponse>()
//         {
//             Success=false,
//             Message="",
//             Data=null
//         };

//         if(product != null)
//         {
//             var productResponse=_mapper.Map<ProductResponse>(product);
            
//             response.Success=true;
//             response.Message="Read product successfully";
//             response.Data=productResponse;            
//         }
//         return response;
//     }

// }

using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Queries;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ApiResponse<ProductResponse>>
{
    private readonly IProductService _productService;

    public GetProductByIdQueryHandler(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        return await _productService.GetProductByIdAsync(query.ProductId, cancellationToken);
    }
}