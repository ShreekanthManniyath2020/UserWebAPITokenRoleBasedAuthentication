namespace WebAPIJwtAuth.Application.DTOs
{
    public class RefreshTokenRequest
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}