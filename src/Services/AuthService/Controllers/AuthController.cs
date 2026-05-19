using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _context;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(AuthDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest,JwtService jwtService)
    {
        var user=_context.Users.FirstOrDefault(u => u.Username==loginRequest.Username);

        if(user==null)
            return Unauthorized("Invalid credentials");
        
        var passwordHasher=new PasswordHasher<User>();

        var result=passwordHasher.VerifyHashedPassword(user,user.PasswordHash,loginRequest.Password);

        if(result==PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credential");

        var token=jwtService.GenerateToken(user);
        return Ok(new {token});
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == request.Username);

        if (existingUser != null)
        {
            return BadRequest("Username already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }
}