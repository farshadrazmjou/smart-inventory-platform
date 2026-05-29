namespace ProductService.Application.DTOs;

public class CreateProductRequest
{
    public string Name{get;set;}=string.Empty;

    public float Price{get;set;}

    public int Stock{get;set;}

    public string Describtion{get;set;}=string.Empty;
}