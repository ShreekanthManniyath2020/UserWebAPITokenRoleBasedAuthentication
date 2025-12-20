using LoginMCVWebAPI.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Services
{
    public class AuthApiClient : IAuthApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string TokenKey = "JwtToken";
        private const string RefreshTokenKey = "RefreshToken";

        public AuthApiClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);
            return await HandleResponse<AuthResponse>(response);
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
            return await HandleResponse<AuthResponse>(response);
        }

        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync()
        {
            var token = GetToken();
            var refreshToken = GetRefreshToken();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(refreshToken))
                return new ApiResponse<AuthResponse> { Success = false, Message = "No tokens available" };

            var request = new RefreshTokenRequest
            {
                Token = token,
                RefreshToken = refreshToken
            };

            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", request);
            return await HandleResponse<AuthResponse>(response);
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);
            return await HandleResponse<string>(response);
        }

        public async Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);
            return await HandleResponse<string>(response);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync("api/auth/validate");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void SetTokens(string token, string refreshToken)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(TokenKey, token);
                session.SetString(RefreshTokenKey, refreshToken);
            }
        }

        public void ClearTokens()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.Remove(TokenKey);
                session.Remove(RefreshTokenKey);
            }
        }

        private string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(TokenKey);
        }

        private string? GetRefreshToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(RefreshTokenKey);
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new ApiResponse<T> { Success = true, Data = result! };
            }
            else
            {
                var error = JsonSerializer.Deserialize<ErrorResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new ApiResponse<T> { Success = false, Message = error?.Message ?? "An error occurred" };
            }
        }
    }
}
