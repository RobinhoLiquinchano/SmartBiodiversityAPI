using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtnMVC.Services;
using SmartBiodiversityUtnModels.DTOs.Account;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SmartBiodiversityUtnMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiClientService _apiClient;

        public AccountController(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            var response = await _apiClient.PostAsync<LoginRequest, TokenResponseDto>(
                "api/Auth/login",
                request
            );

            if (response == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Correo o contraseña incorrectos."
                );

                return View(request);
            }

            Response.Cookies.Append(
    "AccessToken",
    response.AccessToken,
    new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Lax,
        Expires = DateTimeOffset.UtcNow.AddDays(1)
    });

            Response.Cookies.Append(
                "RefreshToken",
                response.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(response.AccessToken);

            var role = jwtToken.Claims
                .FirstOrDefault(c =>
                    c.Type == ClaimTypes.Role ||
                    c.Type == "role" ||
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                ?.Value;

            if (role == "Administrador")
            {
                return RedirectToAction("Index", "Administracion");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("AccessToken");
            Response.Cookies.Delete("RefreshToken");

            return RedirectToAction("Login");
        }
    }
}