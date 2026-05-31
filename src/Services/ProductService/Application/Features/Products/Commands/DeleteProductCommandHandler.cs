using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Application.Features.Products.Commands;

public class DeleteProductCommandHandler : 
                    IRequestHandler<DeleteProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public DeleteProductCommandHandler(IProductRepository repository,IMapper mapper)
    {
        _repository=repository;
        _mapper=mapper;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var deletedProduct=await _repository.DeleteAsync(request.Id);
        return _mapper.Map<ApiResponse<ProductResponse>>(deletedProduct);
    }
}