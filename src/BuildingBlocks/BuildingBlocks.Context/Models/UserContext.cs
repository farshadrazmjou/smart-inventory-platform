namespace BuildingBlocks.Context.Models;

public sealed class UserContext
{
    public string? UserId { get; set; }

    public string? Username { get; set; }

    public List<string> Roles { get; set; }=new();
}