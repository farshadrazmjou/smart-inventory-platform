using System.Text.Json;
using AutoMapper;
using BuildingBlocks.Observability.Activities;
using ProductService.Application.Common;
using ProductService.Application.DTOs;
using ProductService.Application.Events;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Caching;
using ProductService.Infrastructure.Messaging;

namespace ProductService.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _redisCacheService;
    private readonly IRabbitMqPublisher _rabbitMqPublisher;
    private readonly ILogger<ProductService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(
        IProductRepository repository,
        IOutboxRepository outboxRepository,
        IMapper mapper,
        IRedisCacheService redisCacheService,
        IRabbitMqPublisher rabbitMqPublisher,
        ILogger<ProductService> logger, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _outboxRepository = outboxRepository;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
        _rabbitMqPublisher = rabbitMqPublisher;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllProductsAsync(
        ProductQueryParameter parameter,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(InventoryActivity.Product, "Get All Products");

        try
        {
            activity?
                .SetTag("product.page", parameter.Page)
                .SetTag("product.page_size", parameter.PageSize)
                .Event("Get All Products Started");

            var (Items, TotalCount) = await _repository.GetAllAsync(parameter, cancellationToken);

            var response = new PagedResponse<ProductResponse>
            {
                Items = _mapper.Map<List<ProductResponse>>(Items),
                Page = parameter.Page,
                PageSize = parameter.PageSize,
                TotalCount = TotalCount
            };

            activity?
                .SetTag("product.total_count", TotalCount)
                .SetTag("product.result_count", Items.Count)
                .Event("Products Retrieved")
                .Success();

            _logger.LogInformation(
                "Products fetched successfully. Page: {Page}, PageSize: {PageSize}, TotalCount: {TotalCount}",
                parameter.Page,
                parameter.PageSize,
                TotalCount);

            return response;
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("request.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();

            _logger.LogError(
                ex,
                "Error fetching products");

            throw;
        }
    }

    // public async Task<ApiResponse<ProductResponse>> CreateProductAsync(
    //     CreateProductRequest request,
    //     CancellationToken cancellationToken)
    // {
    //     using var activity = ActivityFactory.Start(InventoryActivity.Product, "Create Product");

    //     try
    //     {
    //         activity?.SetTag("product.name", request.Name).SetTag("product.price", request.Price);

    //         // ---------------- Create Product ----------------

    //         var product = _mapper.Map<Product>(request);
    //         var createdProduct = await _repository.AddAsync(product, cancellationToken);

    //         activity?.SetTag("product.id", createdProduct.Id).Event("Product Created");

    //         // ---------------- Invalidate Cache ----------------

    //         await _redisCacheService.RemoveProductCachesAsync(cancellationToken);

    //         activity?.Event("Product Cache Invalidated");

    //         // ---------------- Create Event ----------------

    //         var productCreatedEvent = new ProductCreatedEvent
    //         {
    //             Id = createdProduct.Id,
    //             Name = createdProduct.Name,
    //             Price = createdProduct.Price,
    //             CreatedAt = DateTime.UtcNow
    //         };

    //         // ---------------- RabbitMQ ----------------

    //         await _rabbitMqPublisher.PublishAsync(queueName: "product-created", message: productCreatedEvent, cancellationToken);

    //         activity?.Event("Product Created Event Published");

    //         // ---------------- Outbox ----------------

    //         var outbox = new OutboxMessage
    //         {
    //             Id = Guid.NewGuid(),
    //             Type = nameof(productCreatedEvent),
    //             Payload = JsonSerializer.Serialize(productCreatedEvent),
    //             CreatedAt = DateTime.UtcNow,
    //             Processed = false
    //         };

    //         await _outboxRepository.AddAsync(outbox,cancellationToken);

    //         activity?.Event("Outbox Message Created").Success();

    //         _logger.LogInformation("Product {ProductId} created successfully", createdProduct.Id);

    //         return new ApiResponse<ProductResponse>
    //         {
    //             Data = _mapper.Map<ProductResponse>(createdProduct),
    //             Message = "Product created successfully",
    //             Success = true
    //         };
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         activity?.SetTag("request.cancelled", true);
    //         throw;
    //     }
    //     catch (Exception ex)
    //     {
    //         activity?.AddException(ex).Error();
    //         _logger.LogError(ex, "Error creating product");
    //         throw;
    //     }
    // }

    public async Task<ApiResponse<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(InventoryActivity.Product, "Create Product");

        try
        {
            activity?
                .SetTag("product.name", request.Name)
                .SetTag("product.price", request.Price);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            // ---------------- Create Product ----------------

            var product = _mapper.Map<Product>(request);
            var createdProduct = await _repository.AddAsync(product, cancellationToken);

            activity?
                .SetTag("product.id", createdProduct.Id)
                .Event("Product Created");

            // ---------------- Create Event ----------------

            var productCreatedEvent = new ProductCreatedEvent
            {
                Id = createdProduct.Id,
                Name = createdProduct.Name,
                Price = createdProduct.Price,
                CreatedAt = DateTime.UtcNow
            };

            // ---------------- Create Outbox ----------------

            var outbox = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = nameof(ProductCreatedEvent),
                Payload = JsonSerializer.Serialize(
                    productCreatedEvent),
                CreatedAt = DateTime.UtcNow,
                Processed = false
            };

            await _outboxRepository.AddAsync(outbox, cancellationToken);

            activity?.Event("Outbox Message Created");

            // ---------------- Commit ----------------

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            activity?.Event("Product Transaction Committed");

            // ---------------- Cache Invalidation ----------------

            await _redisCacheService.RemoveProductCachesAsync(cancellationToken);

            activity?.Event("Product Cache Invalidated");

            _logger.LogInformation(
                "Product {ProductId} created successfully",
                createdProduct.Id);

            activity?.Success();

            return new ApiResponse<ProductResponse>
            {
                Data = _mapper.Map<ProductResponse>(createdProduct),
                Message = "Product created successfully",
                Success = true
            };
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("request.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            activity?.AddException(ex).Error();
            _logger.LogError(ex,"Error creating product");
            throw;
        }
    }


    public async Task<ApiResponse<ProductResponse>> GetProductByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(InventoryActivity.Product, "Get Product By Id");

        try
        {
            activity?.SetTag("product.id", productId);
            var product = await _repository.GetByIdAsync(productId, cancellationToken);

            if (product is null)
            {
                activity?
                    .SetTag("product.found", false)
                    .Event("Product Not Found")
                    .Success();

                _logger.LogInformation(
                    "Product {ProductId} was not found",
                    productId);

                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "Product not found",
                    Data = null
                };
            }

            activity?.SetTag("product.found", true).Event("Product Found");

            var productResponse = _mapper.Map<ProductResponse>(product);

            activity?.Success();

            _logger.LogInformation(
                "Product {ProductId} fetched successfully",
                productId);

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Read product successfully",
                Data = productResponse
            };
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("request.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();
            _logger.LogError(ex, "Error fetching product {ProductId}", productId);
            throw;
        }
    }

    public async Task<ApiResponse<ProductResponse>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(InventoryActivity.Product, "Update Product");
        try
        {
            activity?.SetTag("product.id", id).Event("Update Product Started");
            var productForUpdate = _mapper.Map<Product>(request);
            var updatedProduct = await _repository.UpdateAsync(id, productForUpdate, cancellationToken);

            if (updatedProduct is null)
            {
                activity?.SetTag("product.found", false).Event("Product Not Found").Success();
                _logger.LogWarning("Product {ProductId} was not found for update", id);

                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "Product not found",
                    Data = null
                };
            }

            activity?.SetTag("product.found", true).Event("Product Updated");

            // Cache invalidation
            await _redisCacheService.RemoveProductCachesAsync(cancellationToken);
            activity?.Event("Product Cache Invalidated");
            _logger.LogInformation("Product {ProductId} updated successfully and cache invalidated", id);

            var response = _mapper.Map<ProductResponse>(updatedProduct);

            activity?.Success();

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Update completed successfully",
                Data = response
            };
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("request.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw;
        }
    }

    public async Task<ApiResponse<ProductResponse>> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var activity = ActivityFactory.Start(InventoryActivity.Product, "Delete Product");

        try
        {
            activity?.SetTag("product.id", id).Event("Delete Product Started");

            var deletedProduct = await _repository.DeleteAsync(id, cancellationToken);

            if (deletedProduct is null)
            {
                activity?.SetTag("product.found", false).Event("Product Not Found").Success();
                _logger.LogWarning("Product {ProductId} was not found for deletion", id);

                return new ApiResponse<ProductResponse>
                {
                    Success = false,
                    Message = "Product not found",
                    Data = null
                };
            }

            activity?.SetTag("product.found", true).Event("Product Deleted");

            // Cache invalidation
            await _redisCacheService.RemoveProductCachesAsync(cancellationToken);

            activity?.Event("Product Cache Invalidated");
            _logger.LogInformation("Product {ProductId} deleted successfully and cache invalidated", id);

            var response = _mapper.Map<ProductResponse>(deletedProduct);

            activity?.Success();

            return new ApiResponse<ProductResponse>
            {
                Success = true,
                Message = "Product deleted successfully",
                Data = response
            };
        }
        catch (OperationCanceledException)
        {
            activity?.SetTag("request.cancelled", true);
            throw;
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw;
        }
    }

}