using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Mappings;

public class ProductProfile:Profile
{
    public ProductProfile()
    {
        CreateMap<CreateProductRequest,Product>();
        CreateMap<Product,ProductResponse>();
    }
}