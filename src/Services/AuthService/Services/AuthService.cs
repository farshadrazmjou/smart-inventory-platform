using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using BuildingBlocks.Observability.Activities;
using BuildingBlocks.Observability.Factories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public sealed class AuthService : IAuthService
{
    private readonly AuthDbContext _context;

    private readonly JwtService _jwtService;

    private readonly PasswordHasher<User> _passwordHasher;

    private readonly IActivityFactory _activityFactory;

    public AuthService(AuthDbContext context, JwtService jwtService, IActivityFactory activityFactory)
    {
        _context = context;
        _jwtService = jwtService;
        _activityFactory = activityFactory;
        _passwordHasher = new PasswordHasher<User>();
    }
    public async Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activityFactory.Start(InventoryActivity.Auth,"User Login");
        activity?.Username(request.Username);
        try
        {
            var user = await ValidateUserAsync(request, cancellationToken);
            VerifyPassword(user, request.Password);
            var token = GenerateJwt(user);
            activity?.UserId(user.Id).LoginResult("success").Success();

            return token;
        }
        catch (Exception ex)
        {
            activity.Exception(ex);
            throw;
        }
    }

    private async Task<User> ValidateUserAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        using var activity = _activityFactory.Start(InventoryActivity.Auth,"User validation");

        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == request.Username,cancellationToken);

        if (user is null)
        {
            activity?.LoginResult("user_not_found").Error();
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        activity?.UserId(user.Id).Username(user.Username).Event("User Found").Success();

        return user;
    }


    private void VerifyPassword(User user, string password)
    {
        using var activity = _activityFactory.Start(InventoryActivity.Auth,"Verify password");
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            activity?.LoginResult("invalid_password").Error();
            throw new UnauthorizedAccessException("Invalid credentials");
        }
        activity?.Event("Password Verified").Success();
    }

    private string GenerateJwt(User user)
    {
        using var activity = _activityFactory.Start(InventoryActivity.Auth,"Generate JWT token");
        var token = _jwtService.GenerateToken(user);
        activity?.JwtIssued().Success();
        return token;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activityFactory.Start(InventoryActivity.Auth,"User Register");
        try
        {
            using var checkUserActivity = _activityFactory.Start(
                InventoryActivity.Auth,
                "Check Existing User");

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Username == request.Username,cancellationToken);

            if (existingUser != null)
            {
                checkUserActivity?.LoginResult("username_exists").Error();
                activity?.LoginResult("username_exists").Error();
                throw new InvalidOperationException("Username already exists.");
            }

            checkUserActivity?.Event("Username Available").Success();

            using var createUserActivity = _activityFactory.Start(
                InventoryActivity.Auth,
                "Create User");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Role = request.Role
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _context.Users.Add(user);

            await _context.SaveChangesAsync(cancellationToken);

            createUserActivity?
                .UserId(user.Id)
                .Username(user.Username)
                .Event("User Created")
                .Success();

            activity?
                .UserId(user.Id)
                .Username(user.Username)
                .LoginResult("success")
                .Success();
        }
        catch (Exception ex)
        {
            activity?.AddException(ex).Error();
            throw;
        }
    }
}