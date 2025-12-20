using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebAPIJwtAuth.Infrastructure.Extensions
{
    public static class SessionExtensions
    {
        // Token keys - centralized for consistency
        //public const string JwtTokenKey = "JwtToken";
        //public const string RefreshTokenKey = "RefreshToken";
        //public const string UserIdKey = "UserId";
        //public const string UserEmailKey = "UserEmail";
        //public const string UserNameKey = "UserName";
        //public const string TokenExpiryKey = "TokenExpiry";

        //// Set JWT token
        //public static void SetJwtToken(this ISession session, string token)
        //{
        //    session.SetString(JwtTokenKey, token);
        //}

        //// Get JWT token
        //public static string? GetJwtToken(this ISession session)
        //{
        //    return session.GetString(JwtTokenKey);
        //}

        //// Set refresh token
        //public static void SetRefreshToken(this ISession session, string refreshToken)
        //{
        //    session.SetString(RefreshTokenKey, refreshToken);
        //}

        //// Get refresh token
        //public static string? GetRefreshToken(this ISession session)
        //{
        //    return session.GetString(RefreshTokenKey);
        //}

        //// Set user info
        //public static void SetUserInfo(this ISession session, Guid userId, string email, string username)
        //{
        //    session.SetString(UserIdKey, userId.ToString());
        //    session.SetString(UserEmailKey, email);
        //    session.SetString(UserNameKey, username);
        //}

        //// Get user ID
        //public static Guid? GetUserId(this ISession session)
        //{
        //    var userIdStr = session.GetString(UserIdKey);
        //    if (Guid.TryParse(userIdStr, out var userId))
        //    {
        //        return userId;
        //    }
        //    return null;
        //}

        //// Get user email
        //public static string? GetUserEmail(this ISession session)
        //{
        //    return session.GetString(UserEmailKey);
        //}

        //// Get username
        //public static string? GetUserName(this ISession session)
        //{
        //    return session.GetString(UserNameKey);
        //}

        //// Set token expiry
        //public static void SetTokenExpiry(this ISession session, DateTime expiry)
        //{
        //    session.SetString(TokenExpiryKey, expiry.ToString("O")); // ISO 8601 format
        //}

        //// Get token expiry
        //public static DateTime? GetTokenExpiry(this ISession session)
        //{
        //    var expiryStr = session.GetString(TokenExpiryKey);
        //    if (DateTime.TryParse(expiryStr, out var expiry))
        //    {
        //        return expiry;
        //    }
        //    return null;
        //}

        //// Check if token is expired
        //public static bool IsTokenExpired(this ISession session)
        //{
        //    var expiry = session.GetTokenExpiry();
        //    return !expiry.HasValue || expiry.Value <= DateTime.UtcNow;
        //}

        //// Check if token exists and is valid
        //public static bool HasValidToken(this ISession session)
        //{
        //    var token = session.GetJwtToken();
        //    var expiry = session.GetTokenExpiry();

        //    return !string.IsNullOrEmpty(token) &&
        //           expiry.HasValue &&
        //           expiry.Value > DateTime.UtcNow;
        //}

        //// Clear all auth data from session
        //public static void ClearAuthData(this ISession session)
        //{
        //    session.Remove(JwtTokenKey);
        //    session.Remove(RefreshTokenKey);
        //    session.Remove(UserIdKey);
        //    session.Remove(UserEmailKey);
        //    session.Remove(UserNameKey);
        //    session.Remove(TokenExpiryKey);
        //}

        //// Get all session data as dictionary (for debugging)
        //public static Dictionary<string, string> GetSessionData(this ISession session)
        //{
        //    var data = new Dictionary<string, string>();

        //    foreach (var key in session.Keys)
        //    {
        //        var value = session.GetString(key);
        //        if (value != null)
        //        {
        //            data[key] = value;
        //        }
        //    }

        //    return data;
        //}

        //// Complex object storage (for storing user claims, etc.)
        //public static void SetObject<T>(this ISession session, string key, T value)
        //{
        //    var json = JsonSerializer.Serialize(value);
        //    session.SetString(key, json);
        //}

        //public static T? GetObject<T>(this ISession session, string key)
        //{
        //    var json = session.GetString(key);
        //    return json == null ? default : JsonSerializer.Deserialize<T>(json);
        //}
    }
}
