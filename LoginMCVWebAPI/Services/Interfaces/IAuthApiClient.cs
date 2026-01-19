using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Services.Interfaces
{
    public interface IAuthApiClient
    {
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request);

        Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request);

        Task<ApiResponse<AuthResponse>> RefreshTokenAsync();

        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request);

        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request);

        Task<bool> ValidateTokenAsync(string token);

        void SetTokens(string token, string refreshToken,Guid guid);

        void ClearTokens();
    }
}