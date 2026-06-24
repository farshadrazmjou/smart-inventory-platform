using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Queries;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.UnitTests;

public class GetAllProductQueryHandlerTest
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<GetAllProductsQueryHandler>> _loggerMock;
    
    public GetAllProductQueryHandlerTest()
    {
        _repositoryMock=new();
        _mapperMock=new();
        _loggerMock=new();
    }

    [Fact]
    public async Task GetAllProducts_Returns_Data_Successfully()
    {
        // Arrange

        var parameter = new ProductQueryParameter()
        {
            Page = 1,
            PageSize = 10
        };

        var query = new GetAllProductsQuery(parameter);

        var products = new List<Product>()
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "SSD",
                Price = 100,
                Stock = 5
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "RAM",
                Price = 50,
                Stock = 10
            }
        };

        var responses = new List<ProductResponse>()
        {
            new()
            {
                Id = products[0].Id,
                Name = products[0].Name,
                Price = (decimal)products[0].Price,
                Stock = products[0].Stock
            },
            new()
            {
                Id = products[1].Id,
                Name = products[1].Name,
                Price = (decimal)products[1].Price,
                Stock = products[1].Stock
            }
        };

        _repositoryMock
            .Setup(x => x.GetAllAsync(parameter))
            .ReturnsAsync((products, 2));

        _mapperMock
            .Setup(x => x.Map<List<ProductResponse>>(products))
            .Returns(responses);

        var handler = new GetAllProductsQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);

        // Act

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert

        Assert.NotNull(result);

        Assert.Equal(2, result.Items.Count);

        Assert.Equal(2, result.TotalCount);

        Assert.Equal(1, result.Page);

        Assert.Equal(10, result.PageSize);

        _repositoryMock.Verify(
            x => x.GetAllAsync(parameter),
            Times.Once);
    }

    [Fact]
    public async Task GetAllProducts_Returns_Empty_List_When_No_Data_Exists()
    {
        // Arrange

        var parameter = new ProductQueryParameter()
        {
            Page = 1,
            PageSize = 10
        };

        var query = new GetAllProductsQuery(parameter);

        var products = new List<Product>();

        var responses = new List<ProductResponse>();

        _repositoryMock
            .Setup(x => x.GetAllAsync(parameter))
            .ReturnsAsync((products, 0));

        _mapperMock
            .Setup(x => x.Map<List<ProductResponse>>(products))
            .Returns(responses);

        var handler = new GetAllProductsQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object);

        // Act

        var result = await handler.Handle(
            query,
            CancellationToken.None);

        // Assert

        Assert.NotNull(result);

        Assert.Empty(result.Items);

        Assert.Equal(0, result.TotalCount);

        Assert.Equal(1, result.Page);

        Assert.Equal(10, result.PageSize);

        _repositoryMock.Verify(
            x => x.GetAllAsync(parameter),
            Times.Once);
    }

}