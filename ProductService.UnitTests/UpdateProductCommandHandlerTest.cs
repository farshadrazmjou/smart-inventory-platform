using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;

namespace ProductService.UnitTests;

public class UpdateProductCommandHandlerTest
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly Mock<ILogger<UpdateProductCommandHandler>> _loggerMock;

    public UpdateProductCommandHandlerTest()
    {
        _repositoryMock=new();
        _mapperMock=new();
        _redisCacheServiceMock=new();
        _loggerMock=new();
    }

    [Fact]
    public async Task Update_Should_UpdateProduct_Successfully()
    {
        // Arrange
        var productIdforUpdate=Guid.NewGuid();

        var product=new Product()
        {
            Id=productIdforUpdate,
            Name="CPU",
            Price=45,
            Stock=5,
            Description="AMD"
        };

        var updateRequest=new UpdateProductRequest()
        {
            Name="CPU",
            Price=45,
            Stock=5,
            Describtion="AMD"
        };

        _mapperMock
            .Setup(x => x.Map<Product>(It.IsAny<UpdateProductRequest>()))
            .Returns(product);

        _repositoryMock
            .Setup(x => x.UpdateAsync(productIdforUpdate,product))
            .ReturnsAsync(product);

        var command=new UpdateProductCommand(productIdforUpdate,updateRequest);
        var handler=new UpdateProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _redisCacheServiceMock.Object,
                _loggerMock.Object);
        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        
        _repositoryMock.Verify( x => x.UpdateAsync(productIdforUpdate,product),Times.Once());

    }

        [Fact]
    public async Task Update_Should_RemoveCache()
    {
        // Arrange
        var productIdforUpdate=Guid.NewGuid();

        var product=new Product()
        {
            Id=productIdforUpdate,
            Name="CPU",
            Price=45,
            Stock=5,
            Description="AMD"
        };

        var updateRequest=new UpdateProductRequest()
        {
            Name="CPU",
            Price=45,
            Stock=5,
            Describtion="AMD"
        };

        _mapperMock
            .Setup(x => x.Map<Product>(It.IsAny<UpdateProductRequest>()))
            .Returns(product);

        _repositoryMock
            .Setup(x => x.UpdateAsync(productIdforUpdate,product))
            .ReturnsAsync(product);

        var command=new UpdateProductCommand(productIdforUpdate,updateRequest);
        var handler=new UpdateProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _redisCacheServiceMock.Object,
                _loggerMock.Object);
        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert
        _redisCacheServiceMock.Verify(x => x.RemoveProductCachesAsync(),Times.Once());
    }

}