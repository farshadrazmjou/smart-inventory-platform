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

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] " +
        "[CorrelationId: {CorrelationId}] " +
        "{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate:
        "{Timestamp:yyyy-MM-dd HH:mm:ss} " +
        "[{Level:u3}] " +
        "[CorrelationId: {CorrelationId}] " +
        "{Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddControllers();

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
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

builder.Services.AddScoped<IRedisCacheService,RedisCacheService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddAutoMapper(typeof(ProductProfile));

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging();

app.UseMiddleware<ExceptionMiddleware>();

app.UseSerilogRequestLogging();

app.UseAuthentication();
app.UseAuthorization();

// swagger
app.UseSwagger();
app.UseSwaggerUI();

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

app.Run();


