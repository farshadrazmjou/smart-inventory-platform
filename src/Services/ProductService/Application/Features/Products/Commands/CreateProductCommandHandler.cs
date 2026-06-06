using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;

namespace ProductService.Application.Features.Products.Commands;

public class CreateProductCommandHandler:
                    IRequestHandler<CreateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public CreateProductCommandHandler(IProductRepository repository,IMapper mapper,IRedisCacheService redisCacheService,ILogger<CreateProductCommandHandler> logger)
    {
        _repository=repository;
        _mapper=mapper;
        _redisCacheService=redisCacheService;
        _logger=logger;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(CreateProductCommand command,CancellationToken cancellationToken)
    {
        var product=_mapper.Map<Product>(command.CreateProductRequest);

        var createdProduct=await _repository.AddAsync(product);
        await _redisCacheService.RemoveProductCachesAsync();
        _logger.LogInformation("Cache invalidation executed");
        
        return new ApiResponse<ProductResponse>()
        {
            Data=_mapper.Map<ProductResponse>(createdProduct),
            Message="Product created successfully",
            Success=true
        };
    }
}