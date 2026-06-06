using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;

namespace ProductService.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : 
                    IRequestHandler<UpdateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<UpdateProductCommandHandler> _logger;

    public UpdateProductCommandHandler(IProductRepository repository,IMapper mapper,IRedisCacheService redisCacheService,ILogger<UpdateProductCommandHandler> logger)
    {
        _repository=repository;
        _mapper=mapper;
        _redisCacheService=redisCacheService;
        _logger=logger;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productForUpdate=_mapper.Map<Product>(request.Request);
        var UpdatedProduct= await _repository.UpdateAsync(request.Id,productForUpdate);
        await _redisCacheService.RemoveProductCachesAsync();
        _logger.LogInformation("CACHE INVALIDATION STARTED");
        return new ApiResponse<ProductResponse>()
        {
            Data=_mapper.Map<ProductResponse>(UpdatedProduct),
            Message="Update complete successfully",
            Success=true
        };
    }
}