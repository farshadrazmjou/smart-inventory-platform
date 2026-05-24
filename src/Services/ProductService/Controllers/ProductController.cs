using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route(template: "api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductDbContext _context;

    public ProductController(ProductDbContext context)
    {
        _context=context;
    }

    [Authorize]
    [HttpGet]
    public IActionResult GetAll()
    {
        var products=_context.Products.ToList();
        return Ok(value: products);
    }

    [Authorize(Roles ="Admin")]
    [HttpPost]
    public IActionResult Create(Product product)
    {
        product.Id=Guid.NewGuid();
        _context.Products.Add(entity: product);
        _context.SaveChanges();
        return Ok(value: product);
    }

}