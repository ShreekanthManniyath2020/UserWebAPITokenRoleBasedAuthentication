using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Domain.Entities;

namespace WebAPIJwtAuth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);

        Task<TokenResponseDto?> LoginAsync(UserDto request);

        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
    }
}