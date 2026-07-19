using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;

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

    }
}
