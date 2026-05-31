using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Features.Products.Queries;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.API.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductController(IMediator mediator)
    {
        _mediator=mediator;
    }

    [Authorize]
    [HttpGet("GetAllProducts")]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameter productQueryParameter)
    {
        var products=await _mediator.Send(new GetAllProductsQuery(productQueryParameter));
        return Ok(value: new ApiResponse<PagedResponse<ProductResponse>>
        {
            Success=true,
            Message= "Products fetch successfully",
            Data=products
        });
    }

    [Authorize(Roles ="Admin")]
    [HttpPost("CreateProduct")]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var result =await _mediator.Send(new CreateProductCommand(CreateProductRequest: request));
        return Ok(value: result);
    }

    [Authorize(Roles ="Admin")]
    [HttpPost("UpdateProduct")]
    public async Task<IActionResult> Update(Guid Id,UpdateProductRequest updateProductRequest)
    {
        var result=await _mediator.Send(new UpdateProductCommand(Id,updateProductRequest));
        return Ok(result);
    }

    [Authorize(Roles ="Admin")]
    [HttpPost("DeleteProduct")]
    public async Task<IActionResult> Delete(Guid Id)
    {
        var result=await _mediator.Send(new DeleteProductCommand(Id));
        return Ok(result);
    }
}