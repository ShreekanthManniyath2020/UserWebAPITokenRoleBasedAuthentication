using LoginMCVWebAPI.Services.Interfaces;
using System.Security.Claims;

namespace LoginMCVWebAPI.Middleware
{
    public class TokenRefreshMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAuthApiClient _authApiClient;

        public TokenRefreshMiddleware(RequestDelegate next, IAuthApiClient authApiClient)
        {
            _next = next;
            _authApiClient = authApiClient;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string? token = null;
            // Skip token refresh for auth endpoints
            if (context.Request.Path.StartsWithSegments("/Account/Login") ||
                context.Request.Path.StartsWithSegments("/Account/Register") ||
                context.Request.Path.StartsWithSegments("/Account/ForgotPassword") ||
                context.Request.Path.StartsWithSegments("/Account/ResetPassword"))
            {
                await _next(context);
                return;
            }

            if(context.Session != null)
                token = context.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                var isValid = await _authApiClient.ValidateTokenAsync(token);

                if (!isValid)
                {
                    var result = await _authApiClient.RefreshTokenAsync();

                    if (result.Success && result.Data != null)
                    {
                        _authApiClient.SetTokens(result.Data.AccessToken, result.Data.RefreshToken, result.Data.User.Id);

                        // Update claims if needed
                        var claims = new[]
                        {
                        new Claim(ClaimTypes.NameIdentifier, result.Data.User.Id.ToString()),
                        new Claim(ClaimTypes.Email, result.Data.User.Email),
                        new Claim(ClaimTypes.Name, result.Data.User.FirstName),
                        new Claim("Token", result.Data.AccessToken),
                        new Claim("RefreshToken", result.Data.RefreshToken)
                    };

                        var identity = new ClaimsIdentity(claims, "Cookies");
                        context.User = new ClaimsPrincipal(identity);
                    }
                    else
                    {
                        // Clear tokens and redirect to login
                        _authApiClient.ClearTokens();
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    // Extension method
    public static class TokenRefreshMiddlewareExtensions
    {
        public static IApplicationBuilder UseTokenRefresh(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TokenRefreshMiddleware>();
        }
    }
}