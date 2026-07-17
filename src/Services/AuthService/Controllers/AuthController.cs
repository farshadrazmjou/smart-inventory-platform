using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // For test
    private static readonly ActivitySource ActivitySource = new(name: "AuthService.Business");

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
        using var loginActivity = ActivitySource.StartActivity("User Login");

        loginActivity?.SetTag("user.username", loginRequest.Username);
        loginActivity?.SetTag("user.ip",HttpContext.Connection.RemoteIpAddress?.ToString());

        try
        {
            User? user;

            // ---------------- Validate User ----------------

            using (var validateActivity = ActivitySource.StartActivity("Validate User"))
            {
                user = await _context.Users.FirstOrDefaultAsync(x => x.Username == loginRequest.Username);

                if (user == null)
                {
                    validateActivity?.SetStatus(ActivityStatusCode.Error);
                    validateActivity?.SetTag("login.result", "user_not_found");

                    loginActivity?.SetStatus(ActivityStatusCode.Error);

                    return Unauthorized("Invalid credentials");
                }

                validateActivity?.SetTag("user.id", user.Id);
                validateActivity?.AddEvent(new ActivityEvent("User Found"));

                validateActivity?.SetStatus(ActivityStatusCode.Ok);
            }

            loginActivity?.SetTag("user.id", user.Id);

            // ---------------- Verify Password ----------------

            using (var verifyPasswordActivity = ActivitySource.StartActivity("Verify Password"))
            {
                var passwordHasher = new PasswordHasher<User>();

                var result = passwordHasher.VerifyHashedPassword(
                    user,user.PasswordHash, loginRequest.Password);

                if (result == PasswordVerificationResult.Failed)
                {
                    verifyPasswordActivity?.SetStatus(ActivityStatusCode.Error);
                    verifyPasswordActivity?.SetTag("login.result", "invalid_password");

                    loginActivity?.SetStatus(ActivityStatusCode.Error);

                    return Unauthorized("Invalid credentials");
                }

                verifyPasswordActivity?.AddEvent(new ActivityEvent("Password Verified"));

                verifyPasswordActivity?.SetStatus(ActivityStatusCode.Ok);
            }

            // ---------------- Generate JWT ----------------

            string token;

            using (var jwtActivity = ActivitySource.StartActivity("Generate JWT"))
            {
                token = jwtService.GenerateToken(user);
                jwtActivity?.AddEvent(new ActivityEvent("JWT Generated"));
                jwtActivity?.SetStatus(ActivityStatusCode.Ok);
            }

            loginActivity?.SetTag("login.result", "success");
            loginActivity?.SetStatus(ActivityStatusCode.Ok);

            var correlationId = Baggage.GetBaggage("correlation.id");
            var clientIp = Baggage.GetBaggage("client.ip");
            Console.WriteLine($"Correlation = {correlationId}");
            Console.WriteLine($"ClientIp = {clientIp}");

            // Test
            Console.WriteLine("========== BAGGAGE ==========");
            Console.WriteLine(Baggage.GetBaggage("CorrelationId"));
            Console.WriteLine(Baggage.GetBaggage("UserId"));
            Console.WriteLine(Baggage.GetBaggage("Username"));
            Console.WriteLine(Baggage.GetBaggage("Role"));

            return Ok(new { token });
        }
        catch (Exception ex)
        {
            loginActivity?.AddException(ex);
            loginActivity?.SetStatus(ActivityStatusCode.Error, ex.Message);

            throw;
        }
    }

/*
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest,JwtService jwtService)
    {
        // Jaeger span info
        using var userLoginActivity = ActivitySource.StartActivity(name: "User Login");
        userLoginActivity?.SetTag(key: "user.username", value: loginRequest.Username);
        userLoginActivity?.SetTag("user.ip", HttpContext.Connection.RemoteIpAddress?.ToString());

        using var valiateUserActivity = ActivitySource.StartActivity("validate user");
        var user=await _context.Users.FirstOrDefaultAsync(u => u.Username==loginRequest.Username);        
        if(user==null)
        {
            valiateUserActivity?.SetStatus(ActivityStatusCode.Error);
            valiateUserActivity?.SetTag("login.result","user_not_found");
            userLoginActivity?.SetStatus(ActivityStatusCode.Error);
            return Unauthorized("Invalid credentials");
        }
        
        // Jaeger span info
        userLoginActivity?.SetTag("user.id",user.Id);

        var passwordHasher=new PasswordHasher<User>();

        using var verifyPasswordActivity = ActivitySource.StartActivity("Verify Password");
        var result=passwordHasher.VerifyHashedPassword(user,user.PasswordHash,loginRequest.Password);

        if(result==PasswordVerificationResult.Failed)
        {
            valiateUserActivity?.SetStatus(ActivityStatusCode.Error);
            valiateUserActivity?.SetTag("login.result","invalid_password");
            userLoginActivity?.SetStatus(ActivityStatusCode.Error);
            return Unauthorized("Invalid credential");
        }

        valiateUserActivity?.AddEvent(new ActivityEvent("password verified"));

        using var jwtTokenGenerationTokenActivity=ActivitySource.StartActivity("jwt token generation");
        var token=jwtService.GenerateToken(user);        
        jwtTokenGenerationTokenActivity?.AddEvent(new ActivityEvent("JWT token generated"));

        userLoginActivity?.SetStatus(ActivityStatusCode.Ok);
        return Ok(new {token});
    }
*/

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
            Email = request.Email,
            Role=request.Role
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(user, request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return Ok("User registered successfully.");
    }
}