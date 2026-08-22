using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ProductService.Infrastructure.Data;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Repositories;
using ProductService.Application.Mappings;
using FluentValidation.AspNetCore;
using FluentValidation;
using ProductService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Common;
using MediatR;
using ProductService.Application.Features.Products.Queries;
using ProductService.Application.Behaviors;
using ProductService.Infrastructure.Caching;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using ProductService.Infrastructure.Messaging;
using ProductService.Infrastructure.Services.BackgroundServices;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Context.DependenctInjection;
using BuildingBlocks.Context.Extensions;
using BuildingBlocks.Exceptions.DependencyInjection;
using BuildingBlocks.Exceptions.Extensions;
using BuildingBlocks.Observability.DependencyInjection;
using BuildingBlocks.Observability.Tracing;
using BuildingBlocks.MediatR.DependencyInjection;
using ProductService.Infrastructure.Persistence;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Health check
builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration["redis:ConnectionStrings"]!);

// validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequest>();

// DB Context
builder.Services.AddDbContext<ProductDbContext>(optionsAction: op =>
{
    op.UseSqlServer(connectionString: builder.Configuration.GetConnectionString(name: "DefaultConnection"));
});

builder.Services.AddMediatR(typeof(GetAllProductsQueryHandler).Assembly);
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Values
            .SelectMany(v => v.Errors)
            .Select(e => e.ErrorMessage)
            .ToList();

        var response = new ValidationErrorResponse
        {
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

// JWT
builder.Services
    .AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(configureOptions: options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name,

            IssuerSigningKey = new SymmetricSecurityKey(
                key: Encoding.UTF8.GetBytes(s: builder.Configuration[key: "JwtSettings:Key"]!))
        };
    });

// Redis
builder.Services.AddStackExchangeRedisCache(option =>
{
    option.Configuration=builder.Configuration["redis:ConnectionStrings"];
});

builder.Services.AddAuthorization();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(ValidationBehavior<,>));

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(CachingBehavior<,>));

// Register LoggingBehavior, PerformanceBehavior, TracingBehavior
builder.Services.AddInventoryMediatRBehavior();

// RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddScoped<IRabbitMqPublisher,RabbitMqPublisher>();

builder.Services.AddHostedService<OutboxBackgroundService>();
builder.Services.AddSingleton(implementationFactory: sp =>
{
    return sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqSettings>>().Value;
});

builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAutoMapper(typeof(ProductProfile));

// by BuildingBlocks.Exceptions
builder.Services.AddInventoryExceptionHandling();

// by BuildingBlocks.Context
builder.Services.AddInventoryRequestContext();

// by BuildingBlocks.Logging 
builder.Host.AddInventoryLogging(builder.Configuration);

// by BuildingBlocks.Observability
builder.Services.AddInventoryObservability(
    serviceName: builder.Configuration["OpenTelemetry:ServiceName"]!,
    serviceVersion: builder.Configuration["OpenTelemetry:ServiceVersion"]!,
    otlpEndpoint: builder.Configuration["OpenTelemetry:Endpoint"]!,
    activitySources: [ActivityNames.Product,ActivityNames.Redis,ActivityNames.RabbitMq]);

var app = builder.Build();

// by BuildingBlocks.Exceptions
app.UseInventoryExceptionHandler();

//***app.UseMiddleware<CorrelationIdMiddleware>();

// by BuildingBlocks.Context
app.UseInventoryRequestContext();

// by BuildingBlocks.Logging
app.UseInventoryLogging();

app.UseAuthentication();

//app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthorization();
app.MapControllers();

// migration
using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<ProductDbContext>();

    Exception? lastException = null;

    for (int i = 0; i < 10; i++)
    {
        try
        {
            dbContext.Database.Migrate();

            Console.WriteLine("Migration Success");

            lastException = null;
            break;
        }
        catch (Exception ex)
        {
            lastException = ex;

            Console.WriteLine(
                $"Migration Retry {i + 1}/10");

            await Task.Delay(5000);
        }
    }

    if (lastException is not null)
    {
        throw lastException;
    }
}

app.MapHealthChecks( pattern: "/health",options: new HealthCheckOptions
{
     Predicate= _ =>true
});

app.Run();