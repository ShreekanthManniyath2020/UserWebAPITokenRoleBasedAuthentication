using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(RegisterRequest request);

        Task<AuthResponse?> LoginAsync(LoginRequest request);

        Task<AuthResponse?> RefreshTokensAsync(RefreshTokenRequest request);
    }
}