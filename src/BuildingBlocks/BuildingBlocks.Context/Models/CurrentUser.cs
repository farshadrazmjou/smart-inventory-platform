namespace BuildingBlocks.Context.Models;

public sealed class CurrentUser
{
    public string? UserId { get; set; }

    public string? Username { get; set; }

    public string? Role { get; set; }
}