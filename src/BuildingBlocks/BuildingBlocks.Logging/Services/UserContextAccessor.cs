using System.Security.Claims;
using BuildingBlocks.Logging.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Logging.Services;

public sealed class UserContextAccessor : IUserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string? UserId => 
        User?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? Username =>
        User?.FindFirstValue(ClaimTypes.Name);

    public string? Role =>
        User?.FindFirstValue(ClaimTypes.Role);
}