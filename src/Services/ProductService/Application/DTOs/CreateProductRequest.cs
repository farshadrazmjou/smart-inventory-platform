using System.ComponentModel.DataAnnotations;

namespace ProductService.Application.DTOs;

public class CreateProductRequest
{
    [Required]
    public string Name{get;set;}=string.Empty;

    [Range(minimum: 0,maximum: float.MinValue)]
    public float Price{get;set;}

    [Range(minimum: 0,maximum: int.MaxValue)]
    public int Stock{get;set;}

    public string Describtion{get;set;}=string.Empty;
}