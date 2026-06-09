using System.Text.Json;
using AutoMapper;
using MediatR;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Events;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;
using ProductService.Infrastructure.Messaging;

namespace ProductService.Application.Features.Products.Commands;

public class CreateProductCommandHandler:
                    IRequestHandler<CreateProductCommand,ApiResponse<ProductResponse>>
{
    private readonly IProductRepository _repository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;

    public CreateProductCommandHandler(IProductRepository repository,IMapper mapper,IRedisCacheService redisCacheService,ILogger<CreateProductCommandHandler> logger,IRabbitMqPublisher rabbitMqPublisher,IOutboxRepository outboxRepository)
    {
        _repository=repository;
        _mapper=mapper;
        _redisCacheService=redisCacheService;
        _logger=logger;
        _rabbitMqPublisher=rabbitMqPublisher;
        _outboxRepository=outboxRepository;
    }

    public async Task<ApiResponse<ProductResponse>> Handle(CreateProductCommand command,CancellationToken cancellationToken)
    {
        var product=_mapper.Map<Product>(command.CreateProductRequest);

        var createdProduct=await _repository.AddAsync(product);
        await _redisCacheService.RemoveProductCachesAsync();
        
        ProductCreatedEvent productCreatedEvent=new ProductCreatedEvent()
        {
            Id=createdProduct.Id,
            Name=createdProduct.Name,
            Price=createdProduct.Price,
            CreatedAt=DateTime.UtcNow
        };

        var outbox=new OutboxMessage()
        {
            Id=Guid.NewGuid(),
            Type=nameof(productCreatedEvent),
            Payload=JsonSerializer.Serialize(productCreatedEvent),
            CreatedAt=DateTime.UtcNow,
            Processed=false
        };
        await _outboxRepository.AddAsync(outbox);
        //await _rabbitMqPublisher.PublishAsync(queueName: "product-created",message: productCreatedEvent);
        
        return new ApiResponse<ProductResponse>()
        {
            Data=_mapper.Map<ProductResponse>(createdProduct),
            Message="Product created successfully",
            Success=true
        };
    }
}