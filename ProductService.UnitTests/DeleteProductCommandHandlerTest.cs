using AutoMapper;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;

namespace ProductService.UnitTests;

public class DeleteProductCommandHandlerTest
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;

    public DeleteProductCommandHandlerTest()
    {
        _repositoryMock=new();
        _mapperMock=new();
        _redisCacheServiceMock=new();
        _loggerMock=new();
    }

    [Fact]
    public async Task Delete_Should_DeleteProduct_Successfully()
    {
        // Arrange
        var product=new Product()
        {
            Id=Guid.NewGuid(),
            Name="CPU",
            Price=34,
            Stock=5,
            Description="Intel"
        };

        var apiResponse=new ApiResponse<ProductResponse>()
        {
            Message="Delete Complete",
            Success=true,
            Data=new ProductResponse()
            {
                Id=product.Id,
                Description=product.Description,
                Name=product.Name,
                Price=(decimal)product.Price,
                Stock=product.Stock
            }
        };

        _repositoryMock
            .Setup(x => x.DeleteAsync(product.Id))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ApiResponse<ProductResponse>>( It.IsAny<Product>()))
            .Returns(apiResponse);

        var command=new DeleteProductCommand(product.Id);
        var handler=new DeleteProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _redisCacheServiceMock.Object,
                _loggerMock.Object);

        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Asert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        _repositoryMock.Verify(x => x.DeleteAsync(product.Id),Times.Once);
    }

    [Fact]
    public async Task Delete_Should_RemoveCache()
    {
        var product=new Product()
        {
            Id=Guid.NewGuid(),
            Name="CPU",
            Price=34,
            Stock=5,
            Description="Intel"
        };

        var apiResponse=new ApiResponse<ProductResponse>()
        {
            Message="Delete Complete",
            Success=true,
            Data=new ProductResponse()
            {
                Id=product.Id,
                Description=product.Description,
                Name=product.Name,
                Price=(decimal)product.Price,
                Stock=product.Stock
            }
        };

        _repositoryMock
            .Setup(x => x.DeleteAsync(product.Id))
            .ReturnsAsync(product);

        _mapperMock
            .Setup(x => x.Map<ApiResponse<ProductResponse>>( It.IsAny<Product>()))
            .Returns(apiResponse);

        var command=new DeleteProductCommand(product.Id);
        var handler=new DeleteProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _redisCacheServiceMock.Object,
                _loggerMock.Object);

        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert
        _redisCacheServiceMock.Verify(x => x.RemoveProductCachesAsync(),Times.Once);

    }

}