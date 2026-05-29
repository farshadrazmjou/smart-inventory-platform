using ProductService.Application.Interfaces;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using AutoMapper;
using ProductService.Application.Common;

namespace ProductService.Application.Services;

public class ProductService:IProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger _logger;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository,IMapper mapper,ILogger<ProductService> logger)
    {
        _repository=repository;
        _mapper=mapper;
        _logger=logger;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(ProductQueryParameter productQueryParameter)
    {
        var result= await _repository.GetAllAsync(productQueryParameter);
        _logger.LogInformation($"Fetch {result.TotalCount} items of products complete successfullt.");
        return new PagedResponse<ProductResponse>()
        {
            Items=_mapper.Map<List<ProductResponse>>(result.Items),
            Page=productQueryParameter.Page,
            PageSize=productQueryParameter.PageSize,
            TotalCount=result.TotalCount
        };
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        Product product=_mapper.Map<Product>(source: request);
        
        var createdProduct= await _repository.AddAsync(product);
        _logger.LogInformation($"Create product {createdProduct.Name} successfully.");
        return _mapper.Map<ProductResponse>(source: createdProduct);
    }
}