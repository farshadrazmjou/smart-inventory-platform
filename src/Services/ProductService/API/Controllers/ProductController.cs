using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.API.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService=productService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameter productQueryParameter)
    {
        var products=await _productService.GetAllAsync(productQueryParameter);
        return Ok(value: new ApiResponse<PagedResponse<ProductResponse>>
        {
            Success=true,
            Message= "Products fetch successfully",
            Data=products
        });
    }

    [Authorize(Roles ="Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product=await _productService.CreateAsync(request);
        return Ok(value: new ApiResponse<ProductResponse>{
             Success=true,
             Message= "Product created successfully",
             Data=product
        });
    }

}