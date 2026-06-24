using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Queries;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests;

public class GetProductByIdQueryHandlerTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetProductByIdQueryHandler>> _loggerMock;

    public GetProductByIdQueryHandlerTest()
    {
        _productRepositoryMock=new();
        _mapperMock=new();
        _loggerMock=new();
    }
    
    [Fact]
    public async Task GetProductById_Should_Fetch_Product()
    {
        // Arrange
        var productId=Guid.NewGuid();

        var product=new Product()
        {
            Id=productId,
            Name="RAM",
            Price=25,
            Stock=12,
            Description="Accer"
        };

        var productResponse=new ProductResponse()
        {
            Id=productId,
            Name="RAM",
            Price=25,
            Stock=12,
            Description="Accer"
        };        

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(product);        
        
        _mapperMock
            .Setup(x => x.Map<ProductResponse>(product))
            .Returns(productResponse);
        
        var query=new GetProductByIdQuery(productId);

        var handler=new GetProductByIdQueryHandler(
                _productRepositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);

        // Act
        var result=await handler.Handle(query,CancellationToken.None);

        // Assert
        Assert.True(result.Success);

        _productRepositoryMock.Verify(x => x.GetByIdAsync(productId),Times.Once);
    }
}
