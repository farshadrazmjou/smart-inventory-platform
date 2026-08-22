using AuthService.DTOs;
using AuthService.Services;
using BuildingBlocks.Observability.Activities;
using BuildingBlocks.Observability.Factories;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IActivityFactory _activityFactory;

    public AuthController(IAuthService authService, IActivityFactory activityFactory)
    {
        _authService = authService;
        _activityFactory = activityFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        using var activity = _activityFactory.StartCurrent();

        activity?
            .Username(request.Username)
            .ClientIp(HttpContext.Connection.RemoteIpAddress?.ToString());

        try
        {
            var token = await _authService.LoginAsync(request, cancellationToken);
            activity?.LoginResult("success").Success();
            return Ok(new
            {
                token
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            activity?.LoginResult("failed").Error(ex.Message);
            return Unauthorized(ex.Message);
        }
        catch (Exception ex)
        {
            activity.Exception(ex);
            throw;
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _authService.RegisterAsync(request, cancellationToken);
        return Ok("User registered successfully.");
    }
}