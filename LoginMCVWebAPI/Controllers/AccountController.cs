using LoginMCVWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPIJwtAuth.Application.DTOs;

namespace LoginMCVWebAPI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthApiClient _authApiClient;

        public AccountController(IAuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiClient.LoginAsync(model);

            if (result.Success && result.Data != null)
            {
                // Store tokens in session
                _authApiClient.SetTokens(result.Data.AccessToken, result.Data.RefreshToken);

                // Create claims identity
                var claims = new[]
                {
                new Claim(ClaimTypes.NameIdentifier, result.Data.User.Id.ToString()),
                new Claim(ClaimTypes.Email, result.Data.User.Email),
                new Claim(ClaimTypes.Name, result.Data.User.FirstName),
                new Claim(ClaimTypes.Role, result.Data.User.Role),  
                new Claim("Token", result.Data.AccessToken),
                new Claim("RefreshToken", result.Data.RefreshToken)
            };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(15)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Invalid login attempt");
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiClient.RegisterAsync(model);

            if (result.Success && result.Data != null)
            {
                // Auto-login after registration
                return await Login(new LoginRequest
                {
                    Email = model.Email,
                    Password = model.Password
                });
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Registration failed");
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiClient.ForgotPasswordAsync(model);

            if (result.Success)
            {
                ViewBag.Message = "If an account exists with this email, a password reset link has been sent.";
                return View("ForgotPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to process request");
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordRequest
            {
                Token = token,
                Email = email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiClient.ResetPasswordAsync(model);

            if (result.Success)
            {
                ViewBag.Message = "Your password has been reset successfully.";
                return View("ResetPasswordConfirmation");
            }

            ModelState.AddModelError(string.Empty, result.Message ?? "Failed to reset password");
            return View(model);
        }

       // [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _authApiClient.ClearTokens();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}