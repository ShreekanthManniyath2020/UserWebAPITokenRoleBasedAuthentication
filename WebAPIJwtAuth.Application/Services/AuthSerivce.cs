using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebAPIJwtAuth.Application.DTOs;
using WebAPIJwtAuth.Application.Interfaces;
using WebAPIJwtAuth.Domain;
using WebAPIJwtAuth.Domain.Entities;
using WebAPIJwtAuth.Infrastructure.Data;

namespace WebAPIJwtAuth.Application.Services
{
    public class AuthSerivce(ApplicationDbContext context, JWTSettings jWTSettings, IMapper mapper) : IAuthService
    {
        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user is null)
            {
                return null;
            }

            var passwordVerificationResult = new PasswordHasher<User>()
                .VerifyHashedPassword(user, user.PasswordHash, request.Password);
                       
            if (passwordVerificationResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        private async Task<AuthResponse> CreateTokenResponse(User? user)
        {
            return new AuthResponse
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),
                User = mapper.Map<UserDto>(user),
                TokenExpiration = user.TokenExpiration!.Value,
            };
        }

        public async Task<User?> RegisterAsync(RegisterRequest request)
        {
            if (await context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return null;
            }
            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Email = request.Email;
            user.PasswordHash = hashedPassword;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Role = request.Role;

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<AuthResponse?> RefreshTokensAsync(RefreshTokenRequest request)
        {
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken
                || user.TokenExpiration <= DateTime.UtcNow)
            {
                return null;
            }

            return user;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.TokenExpiration = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>{
              new Claim(ClaimTypes.Email ,user.Email),
               new Claim(ClaimTypes.NameIdentifier ,user.Id.ToString()),
               new Claim(ClaimTypes.Role ,user.Role)

            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jWTSettings.SecretKey!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: jWTSettings.Issuer,
                audience: jWTSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes((double)jWTSettings.ExpireMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        
    }
}