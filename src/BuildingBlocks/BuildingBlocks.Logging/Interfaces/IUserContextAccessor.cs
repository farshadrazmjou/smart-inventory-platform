using System.Security.Claims;

namespace BuildingBlocks.Logging.Interfaces;

public interface IUserContextAccessor
{
    bool IsAuthenticated { get; }

    string? UserId { get; }

    string? Username { get; }

    string? Role { get; }

    ClaimsPrincipal? User { get; }
}