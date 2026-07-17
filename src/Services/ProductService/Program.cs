using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using ProductService.Infrastructure.Data;
using ProductService.Application.Interfaces;
using ProductService.Infrastructure.Repositories;
using ProductService.API.Middlewares;
using ProductService.Application.Mappings;
using Serilog;
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
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Context.DependenctInjection;
using BuildingBlocks.Context.Extensions;

// Log.Logger = new LoggerConfiguration()
//     .Enrich.FromLogContext()
//     .WriteTo.Console(
//         outputTemplate:
//         "[{Timestamp:HH:mm:ss} {Level:u3}] " +
//         "[CorrelationId: {CorrelationId}] " +
//         "{Message:lj}{NewLine}{Exception}")
//     .WriteTo.File(
//         path: "Logs/log-.txt",
//         rollingInterval: RollingInterval.Day,
//         outputTemplate:
//         "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
//         "[{Level:u3}] " +
//         "[CorrelationId: {CorrelationId}] " +
//         "{Message:lj}{NewLine}{Exception}")
//     .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
// builder.Host.UseSerilog();

// OpenTelemetry
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource =>
        resource.AddService(
            serviceName: builder.Configuration["OpenTelemetry:ServiceName"]!,
            serviceVersion: builder.Configuration["OpenTelemetry:ServiceVersion"]!))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation(options =>
            {
                options.RecordException = true;
            })
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri(builder.Configuration["OpenTelemetry:Endpoint"]!);

                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    });

builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddRedis(builder.Configuration["redis:ConnectionStrings"]!);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequest>();

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

builder.Services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(ValidationBehavior<,>));

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(LoggingBehavior<,>));

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(CachingBehavior<,>));

builder.Services.AddTransient(
    serviceType: typeof(IPipelineBehavior<,>),
    implementationType: typeof(PerformanceBehavior<,>)
);

// RabbitMQ
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddScoped<IRabbitMqPublisher,RabbitMqPublisher>();
//builder.Services.AddHostedService<ProductCreatedConsumer>();

builder.Services.AddHostedService<OutboxBackgroundService>();
builder.Services.AddSingleton(implementationFactory: sp =>
{
    return sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqSettings>>().Value;
});

builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddAutoMapper(typeof(ProductProfile));

// Serilog by BuildingBlocks 
builder.Host.AddInventoryLogging(builder.Configuration);

// BuildingBlocks.Context
builder.Services.AddRequestContext();

var app = builder.Build();

// Serilog by BuildingBlocks 
app.UseInventoryBaggage();

// BuildingBlocks.Context
app.UseRequestContext();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();

app.UseMiddleware<ExceptionMiddleware>();

app.UseInventoryBaggage();

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