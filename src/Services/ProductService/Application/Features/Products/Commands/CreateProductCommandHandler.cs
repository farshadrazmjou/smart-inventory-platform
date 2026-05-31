using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Features.Products.Commands;

public class CreateProductCommandHandler:
                    IRequestHandler<CreateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IProductRepository repository,IMapper mapper)
    {
        _repository=repository;
        _mapper=mapper;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(CreateProductCommand command,CancellationToken cancellationToken)
    {
        var product=_mapper.Map<Product>(command.CreateProductRequest);

        var createdProduct=await _repository.AddAsync(product);

        return new ApiResponse<ProductResponse>()
        {
            Data=_mapper.Map<ProductResponse>(createdProduct),
            Message="Product created successfully",
            Success=true
        };
    }
}