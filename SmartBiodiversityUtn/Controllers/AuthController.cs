using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthServices authServices) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult<Usuario>> Register(UserDto request)
        {
            var user = await authServices.RegisterAsync(request);
            if (user is null)
            {
                return BadRequest("User already exists.");
            }
            return Ok(new
            {
                idUsuario = user.IdUsuario,
                nombres = user.Nombres,
                apellidos = user.Apellidos,
                correo = user.Correo,
                estado = user.Estado,
                fechaRegistro = user.FechaRegistro
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequest request)
        {
            var result = await authServices.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid username or password");
            }

            return Ok(result);
        }


        //[Authorize]      
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto request)
        {
            var result = await authServices.RefreshTokensAsync(request);

            if (result is null || result.AccessToken is null ||
                result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token or user ID.");
            }
            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(PasswordResetRequest request)
        {
            var token = await authServices.GeneratePasswordResetTokenAsync(request.Email);
            if (token == null)
                return BadRequest("Usuario no encontrado.");

            // Aquí puedes enviar el token por correo (por ahora lo devolvemos)
            return Ok(new { message = "Token de restablecimiento generado", token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            var success = await authServices.ResetPasswordAsync(request.Token, request.NewPassword);
            return success
                ? Ok("Contraseña restablecida exitosamente.")
                : BadRequest("Token inválido o contraseña ya utilizada anteriormente.");
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var success = await authServices.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return success
                ? Ok("Contraseña cambiada exitosamente.")
                : BadRequest("Contraseña actual incorrecta o ya utilizada anteriormente.");
        }
    }
}
