using AuthService.DTOs;

namespace AuthService.Services;

public interface IAuthService
{
    Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
}