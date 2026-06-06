using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Caching;

namespace ProductService.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : 
                    IRequestHandler<DeleteProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<CreateProductCommandHandler> _logger;

    public DeleteProductCommandHandler(IProductRepository repository,IMapper mapper,IRedisCacheService redisCacheService,ILogger<CreateProductCommandHandler> logger)
    {
        _repository=repository;
        _mapper=mapper;
        _redisCacheService=redisCacheService;
        _logger=logger;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var deletedProduct=await _repository.DeleteAsync(request.Id);
        await _redisCacheService.RemoveProductCachesAsync();
        _logger.LogInformation("CACHE INVALIDATION STARTED");
        return _mapper.Map<ApiResponse<ProductResponse>>(deletedProduct);
    }
}