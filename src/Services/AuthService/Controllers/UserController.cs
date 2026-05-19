using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers;

[ApiController]
[Route("/api/[Controller]")]
public class UserController:ControllerBase
{
    [Authorize]    
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {        
        var userId=User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username=User.Identity?.Name;
        var email = User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            UserId = userId,
            Username = username,
            Email = email
        });
    }
}