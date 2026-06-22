using System.Text;
using ApiGateway.Middleware;
using Microsoft.IdentityModel.Tokens;
using Serilog;

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

// Serilog
builder.Host.UseSerilog();

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

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();

app.Run();
