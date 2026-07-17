using System.Text;
using ApiGateway.Middleware;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog.Enrichers.Span;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Context.DependenctInjection;
using BuildingBlocks.Context.Extensions;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(configure: resource => 
        resource.AddService(
            serviceName:builder.Configuration["OpenTelemetry:ServiceName"]!,
            serviceVersion:builder.Configuration["OpenTelemetry:ServiceVersion"]!))
    .WithTracing(configure: tracing => 
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(configure: options =>
            {
                options.Endpoint=new Uri(builder.Configuration["OpenTelemetry:Endpoint"]!);
                options.Protocol=OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            }));

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

// Serilog by BuildingBlocks 
builder.Host.AddInventoryLogging(configuration: builder.Configuration);

// BuildingBlocks.Context
builder.Services.AddRequestContext();

var app = builder.Build();

Log.Information(messageTemplate: "ApiGateway Started Successfully");

// BuildingBlocks.Context
app.UseRequestContext();

// Serilog by BuildingBlocks
app.UseInventoryLogging();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseAuthentication();

// Baggage by BuildingBlocks
app.UseInventoryBaggage();

app.UseAuthorization();
app.MapReverseProxy();

app.Run();
