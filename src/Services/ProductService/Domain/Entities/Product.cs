namespace ProductService.Domain.Entities;

public class Product
{
    public Guid Id {get;set;}=Guid.NewGuid();
    public string Name {get;set;}=string.Empty;
    public float Price {get;set;}
    public int Stock {get;set;}
    public string Describtion {get;set;}=string.Empty;
}