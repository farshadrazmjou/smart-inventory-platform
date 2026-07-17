using AuthService.Data;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using BuildingBlocks.Logging.Extensions;
using BuildingBlocks.Logging.DependencyInjection;
using BuildingBlocks.Context.Extensions;
using BuildingBlocks.Context.DependenctInjection;

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

// OpenTelemetry
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => 
        resource.AddService(
            serviceName:builder.Configuration["OpenTelemetry:ServiceName"]!,
            serviceVersion:builder.Configuration["OpenTelemetry:ServiceVersion"]!
        )).
    WithTracing(trace =>
    {
        trace.
        AddSource("AuthService.Business").
        SetSampler(sampler: new AlwaysOnSampler()).
        AddAspNetCoreInstrumentation().
        AddHttpClientInstrumentation().
        AddSqlClientInstrumentation(option =>
        {
            option.RecordException=true;
        }).
        AddConsoleExporter().
        AddOtlpExporter(option =>
        {
            option.Endpoint=new Uri(builder.Configuration["OpenTelemetry:Endpoint"]!);
            option.Protocol=OtlpExportProtocol.Grpc;
        });
    });

// Serilog by BuildingBlocks 
builder.Host.AddInventoryLogging(configuration: builder.Configuration);

// BuildingBlocks.Context
builder.Services.AddRequestContext();

var app = builder.Build();

// BuildingBlocks.Context
app.UseRequestContext();

// Serilog by BuildingBlocks 
app.UseInventoryLogging();

app.UseAuthentication();

app.UseInventoryBaggage();

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