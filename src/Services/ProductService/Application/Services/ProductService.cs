using ProductService.Application.Interfaces;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;
using AutoMapper;

namespace ProductService.Application.Services;

public class ProductService:IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public ProductService(IProductRepository repository,IMapper mapper)
    {
        _repository=repository;
        _mapper=mapper;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        Product product=_mapper.Map<Product>(request);
        
        return await _repository.AddAsync(product);
    }
}