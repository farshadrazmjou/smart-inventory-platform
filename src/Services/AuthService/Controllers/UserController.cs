using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route(template: "/api/[Controller]")]
public class UserController:ControllerBase
{
    [Authorize]    
    [HttpGet(template: "profile")]
    public IActionResult GetProfile()
    {        
        var userId=User.FindFirstValue(claimType: ClaimTypes.NameIdentifier);
        var username=User.Identity?.Name;
        var email = User.FindFirstValue(claimType: ClaimTypes.Email);
        var role=User.FindFirstValue(claimType: ClaimTypes.Role);

        return Ok(value: new
        {
            UserId = userId,
            Username = username,
            Email = email,
            Role=role
        });
    }
}