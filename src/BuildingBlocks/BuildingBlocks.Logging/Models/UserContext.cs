namespace BuildingBlocks.Logging.Models;

public sealed class UserContext
{
    public bool IsAuthenticated { get; init; }

    public string? UserId { get; init; }

    public string? Username { get; init; }

    public string? Role { get; init; }
}