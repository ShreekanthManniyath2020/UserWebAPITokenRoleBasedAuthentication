using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Services
{
    public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime TokenExpiration { get; set; }
    public UserDto User { get; set; } = null!;
}
}
