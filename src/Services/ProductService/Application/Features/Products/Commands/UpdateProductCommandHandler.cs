using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands;

public class UpdateProductCommandHandler : 
                    IRequestHandler<UpdateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(IProductRepository repository,IMapper mapper)
    {
        _repository=repository;
        _mapper=mapper;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var productForUpdate=_mapper.Map<Product>(request.Request);
        var UpdatedProduct= await _repository.UpdateAsync(request.Id,productForUpdate);
        return new ApiResponse<ProductResponse>()
        {
            Data=_mapper.Map<ProductResponse>(UpdatedProduct),
            Message="Update complete successfully",
            Success=true
        };
    }
}