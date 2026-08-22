using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Features.Products.Queries;

namespace ProductService.API.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductController> _logger;
    public ProductController(IMediator mediator,ILogger<ProductController> logger)
    {
        _mediator=mediator;
        _logger=logger;
    }

    [Authorize]
    [HttpGet("GetAllProducts")]
    public async Task<IActionResult> GetAll([FromQuery] ProductQueryParameter productQueryParameter)
    {
        _logger.LogInformation("===== PRODUCT CONTROLLER =====");
        Console.WriteLine("===== PRODUCT CONTROLLER =====");
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

    [Authorize]
    [HttpGet("GetProductById")]
    public async Task<IActionResult> GetById(Guid Id)
    {
        var result=await _mediator.Send(new GetProductByIdQuery(Id));
        return Ok(result);
    }
}
