using AutoMapper;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Events;
using ProductService.Application.Features.Products.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;
using ProductService.Infrastructure.Messaging;

namespace ProductService.UnitTests;

public class CreateProductCommandHandlerTest
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly Mock<ILogger<CreateProductCommandHandler>> _loggerMock;
    private readonly Mock<IRabbitMqPublisher> _rabbitMock;
    private readonly Mock<IOutboxRepository> _outboxMock;
    
    public CreateProductCommandHandlerTest()
    {
        _repositoryMock = new();
        _mapperMock = new();
        _cacheMock = new();
        _loggerMock = new();
        _rabbitMock = new();
        _outboxMock = new();
    }

    [Fact]
    public async Task Handle_Should_Create_Product_And_Return_Success_Response()
    {
    // Arrange

        var request = new CreateProductRequest
        {
            Name = "SSD",
            Price = 100
        };

        var command = new CreateProductCommand(request);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "SSD",
            Price = 100
        };

        var responseDto = new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = (decimal)product.Price
        };

        _mapperMock.Setup(x => x.Map<Product>(request)).Returns(product);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>())).ReturnsAsync(product);

        _mapperMock.Setup(x => x.Map<ProductResponse>(product)).Returns(responseDto);

        var handler = new CreateProductCommandHandler(
        _repositoryMock.Object,
        _mapperMock.Object,
        _cacheMock.Object,
        _loggerMock.Object,
        _rabbitMock.Object,
        _outboxMock.Object);

        // Act

        var result = await handler.Handle(command,CancellationToken.None);

        // Assert

        Assert.True(result.Success);

        Assert.NotNull(result.Data);

        Assert.Equal("SSD", result.Data.Name);

        _repositoryMock.Verify(x => x.AddAsync(It.IsAny<Product>()), Times.Once);

        _cacheMock.Verify(x => x.RemoveProductCachesAsync() , Times.Once );

        _rabbitMock.Verify( x => x.PublishAsync(
                                "product-created",
                                It.IsAny<ProductCreatedEvent>()),
            Times.Once);

        _outboxMock.Verify(x => x.AddAsync(It.IsAny<OutboxMessage>()) , Times.Once );
    }

    [Fact]
    public async Task Event_Published_With_Correct_Value()
    {
        // Arrange
        var request=new CreateProductRequest()
        {
            Name="Hard",
            Price=20,
            Stock=5,
            Describtion="WD"
        };

        var command=new CreateProductCommand(request);

        var response=new ProductResponse()
        {
            Id=Guid.NewGuid(),
            Name="Hard",
            Price=20,
            Stock=5,
            Description="WD"
        };

        var product=new Product()
        {
            Id=response.Id,
            Name=response.Name,
            Price=(float)response.Price,
            Stock=response.Stock,
            Description=response.Description
        };

        _mapperMock.Setup(x => x.Map<Product>(request))
                    .Returns(product);

        _repositoryMock.Setup(x => x.AddAsync(It.IsAny<Product>() ))
                    .ReturnsAsync(product);
        
        _mapperMock.Setup(x => x.Map<ProductResponse>(product)).Returns(response);

        var handler=new CreateProductCommandHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            _rabbitMock.Object,
            _outboxMock.Object);

        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert

        _rabbitMock.Verify(x => x.PublishAsync(
                    "product-created", 
                    It.Is<ProductCreatedEvent>(
                        e => e.Name==product.Name && 
                        e.Id==product.Id && e.Price==product.Price))
                                    ,Times.Once);

    }

    [Fact]
    public async Task OutBoxMessage_Create_Successfully()
    {
        // Arrange
        var request=new CreateProductRequest()
        {
            Name="HDD",
            Price=10,
            Stock=15,
            Describtion="WD"
        };

        var product=new Product()
        {
            Id=Guid.NewGuid(),
            Name=request.Name,
            Price=request.Price,
            Stock=request.Stock,
            Description=request.Describtion
        };

        var response=new ProductResponse()
        {
            Id=product.Id,
            Name=product.Name,
            Price=(decimal)product.Price,
            Stock=product.Stock,
            Description=product.Description
        };

        var command=new CreateProductCommand(request);

        _mapperMock.Setup(x => x.Map<Product>( request)).Returns(product);
        _mapperMock.Setup(x => x.Map<ProductResponse>(product)).Returns(response);
        _repositoryMock.Setup(x => x.AddAsync( It.IsAny<Product>())).ReturnsAsync(product);
        
        var handler=new CreateProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _cacheMock.Object,
                _loggerMock.Object,
                _rabbitMock.Object,
                _outboxMock.Object);

        ProductCreatedEvent productCreatedEvent=new ProductCreatedEvent();
        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert
        _outboxMock.Verify(x => x.AddAsync( 
                It.Is<OutboxMessage>(
                    m =>  m.Type==nameof(productCreatedEvent) && m.Processed==false)),Times.Once() );
    }

    [Fact]
    public async Task CacheInvalidation_Call_Successfully()
    {
        // Arrange
        var request=new CreateProductRequest()
        {
            Name="HDD",
            Price=10,
            Stock=15,
            Describtion="WD"
        };

        var product=new Product()
        {
            Id=Guid.NewGuid(),
            Name=request.Name,
            Price=request.Price,
            Stock=request.Stock,
            Description=request.Describtion
        };

        _mapperMock.Setup( x => x.Map<Product>(request)).Returns(product);
        _repositoryMock.Setup(x => x.AddAsync( It.IsAny<Product>())).ReturnsAsync(product);
        var command=new CreateProductCommand(request);

        var handler=new CreateProductCommandHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _cacheMock.Object,
                _loggerMock.Object,
                _rabbitMock.Object,
                _outboxMock.Object);

        // Act
        var result=await handler.Handle(command,CancellationToken.None);

        // Assert
        _cacheMock.Verify( x => x.RemoveProductCachesAsync(),Times.Once);
        
    }
}