using System.Text;
using ApiGateway.Middleware;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Context.DependenctInjection;
using BuildingBlocks.Context.Extensions;
using BuildingBlocks.Exceptions.Extensions;
using BuildingBlocks.Exceptions.DependencyInjection;
using BuildingBlocks.Observability.DependencyInjection;
using BuildingBlocks.Observability.Tracing;

var builder = WebApplication.CreateBuilder(args);

// YARP
builder.Services
        .AddReverseProxy()
        .LoadFromConfig(config: builder.Configuration.GetSection("ReverseProxy"));

// JWT
builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]!))
        };
    });

// Policy
builder.Services.AddAuthorization(configure: options =>
{
    options.AddPolicy(name: "Authenticated", configurePolicy: policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

// Http Resilience
builder.Services.AddHttpClient(name: "resilience").AddStandardResilienceHandler();

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
    activitySources: ActivityNames.ApiGateway);

var app = builder.Build();

Log.Information(messageTemplate: "ApiGateway Started Successfully");

// by BuildingBlocks.Exceptions
app.UseInventoryExceptionHandler();

//app.UseMiddleware<CorrelationIdMiddleware>();

// by BuildingBlocks.Context
app.UseInventoryRequestContext();

// by BuildingBlocks.Logging
app.UseInventoryLogging();

app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
