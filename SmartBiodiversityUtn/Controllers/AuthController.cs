using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Helpers;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.DTOs.Account.SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthServices authServices) : ControllerBase
    {

        [HttpPost("send-verification-code")]
        [AllowAnonymous]
        public async Task<IActionResult> SendVerificationCode(SendVerificationCodeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var enviado = await authServices.SendVerificationCodeAsync(request.Email);

            if (!enviado)
            {
                return BadRequest(new
                {
                    message = "No fue posible enviar el código. " +
                              "El correo podría estar registrado o hubo un error al enviar el correo."
                });
            }

            return Ok(new
            {
                message = "Código de verificación enviado correctamente al correo.",
                expiracionMinutos = 1
            });
        }

        [HttpPost("verify-code")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(VerifyCodeRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var valido = await authServices.VerifyRegistrationCodeAsync(
                request.Email, request.Codigo);

            if (!valido)
            {
                return BadRequest(new
                {
                    message = "El código es inválido, ya fue utilizado, fue invalidado o expiró."
                });
            }

            return Ok(new
            {
                message = "Código verificado correctamente. Procede a completar tu registro.",
                verified = true
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<Usuario>> Register(UserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await authServices.RegisterAsync(request);

            if (user is null)
            {
                return BadRequest(new
                {
                    message = "No fue posible registrar el usuario. " +
                              "El correo puede estar registrado, el código puede ser " +
                              "incorrecto, ya fue usado o expiró."
                });
            }

            return Ok(new
            {
                idUsuario = user.IdUsuario,
                nombres = user.Nombres,
                apellidos = user.Apellidos,
                correo = user.Correo,
                estado = user.Estado,
                fechaRegistro = user.FechaRegistro.ToEcuadorTime(),
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Login(LoginRequest request)
        {
            var result = await authServices.LoginAsync(request);
            if (result is null)
            {
                return BadRequest("Invalid username or password");
            }
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(PasswordResetRequest request)
        {
            var token = await authServices.GeneratePasswordResetTokenAsync(request.Email);
            if (token == null)
                return BadRequest("Usuario no encontrado.");

            return Ok(new { message = "Token de restablecimiento generado"});
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
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

            var success = await authServices.ChangePasswordAsync(
                userId, request.CurrentPassword, request.NewPassword);

            return success
                ? Ok("Contraseña cambiada exitosamente.")
                : BadRequest("Contraseña actual incorrecta o ya utilizada anteriormente.");
        }
    }
}
