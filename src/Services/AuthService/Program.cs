using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Context.Extensions;
using BuildingBlocks.Context.DependenctInjection;
using BuildingBlocks.Exceptions.DependencyInjection;
using BuildingBlocks.Exceptions.Extensions;
using BuildingBlocks.Observability.DependencyInjection;
using BuildingBlocks.Observability.Tracing;
using BuildingBlocks.MediatR.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.

// HealthCheck
builder.Services
    .AddHealthChecks()
    .AddSqlServer(connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!);

// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"]!))
    };
});
builder.Services.AddScoped<JwtService>();

// DbContext
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlServer(connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null);
    });
});

// DI
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();

// Register LoggingBehavior, PerformanceBehavior, TracingBehavior
builder.Services.AddInventoryMediatRBehavior();

// by BuildingBlocks.Exceptions
builder.Services.AddInventoryExceptionHandling();

// by BuildingBlocks.Context
builder.Services.AddInventoryRequestContext();

// by BuildingBlocks.Logging
builder.Host.AddInventoryLogging(configuration: builder.Configuration);

// by BuildingBlocks.Observability
builder.Services.AddInventoryObservability(
    serviceName: builder.Configuration["OpenTelemetry:ServiceName"]!,
    serviceVersion: builder.Configuration["OpenTelemetry:ServiceVersion"]!,
    otlpEndpoint: builder.Configuration["OpenTelemetry:Endpoint"]!,
    activitySources: ActivityNames.Auth);

var app = builder.Build();

// by BuildingBlocks.Exceptions
app.UseInventoryExceptionHandler();

// by BuildingBlocks.Context
app.UseInventoryRequestContext();

// by BuildingBlocks.Logging 
app.UseInventoryLogging();

app.UseAuthentication();

//**app.UseInventoryBaggage();

app.UseAuthorization();

app.MapControllers();

// migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

    for (int i = 0; i < 10; i++)
    {
        try
        {
            dbContext.Database.Migrate();
            break;
        }
        catch
        {
            await Task.Delay(5000);
        }
    }
}

app.MapHealthChecks("/Health",new HealthCheckOptions
{
    Predicate= _ => true
});

app.Run();