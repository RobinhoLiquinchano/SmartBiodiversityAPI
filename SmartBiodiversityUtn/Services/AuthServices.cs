using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SmartBiodiversityUtn.Services
{
    public class AuthServices(
        SmartBiodiversityUtnContext context, 
        IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IBitacoraService bitacoraService
        ) : IAuthServices
    {
        public async Task<TokenResponseDto?> LoginAsync(LoginRequest request)
        {
            var user = await context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == request.Email);

            if (user is null) 
            {
                await bitacoraService.RegistrarAccionComoAsync(
                    "SYSTEM",
                    "LOGIN_FALLIDO_USUARIO_INEXISTENTE",
                    $"Intento de login con email: {request.Email}");
                return null;
            }
            

            if (new PasswordHasher<Usuario>().VerifyHashedPassword(user, user.Password, request.Password)
                == PasswordVerificationResult.Failed)
            {
                // LOG: Intento de login fallido - contraseña incorrecta
                await bitacoraService.RegistrarAccionComoAsync(
                    user.IdUsuario,
                    "LOGIN_FALLIDO_CONTRASEÑA",
                    $"Contraseña incorrecta para: {user.Correo}");
                return null;
            }

            // LOG: Login exitoso
            await bitacoraService.RegistrarAccionComoAsync(
                user.IdUsuario,
                "LOGIN",
                $"Inicio de sesión exitoso");

            return await CreateTokenResponse(user);
        }

        public async Task<Usuario?> RegisterAsync(UserDto request)
        {
            if (await context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
            {
                await bitacoraService.RegistrarAccionComoAsync(
                    "SYSTEM",
                    "REGISTRO_FALLIDO_EMAIL_EXISTE",
                    $"Intento de registro con email existente: {request.Correo}");
                return null;
            }

            var user = new Usuario
            {
                IdUsuario = "USR-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                Apellidos = request.Apellidos,
                Nombres = request.Nombres,
                Correo = request.Correo,
                Password = new PasswordHasher<Usuario>().HashPassword(null!, request.Password),
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow,
                IntentosFallidos = 0,
                IdRolesU = "2"
            };

            context.Usuarios.Add(user);
            await context.SaveChangesAsync();

            // LOG: Usuario registrado
            await bitacoraService.RegistrarAccionComoAsync(
                user.IdUsuario,
                "REGISTRO_USUARIO",
                $"Nuevo usuario registrado: {user.Correo} (Rol: Visitante)");

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto)
        {
            var user = await ValidateTokenRefreshTokenAsync(requestDto.UserId, requestDto.RefreshToken);
            if (user is null)
            {
                await bitacoraService.RegistrarAccionComoAsync(
                   requestDto.UserId,
                   "REFRESH_TOKEN_FALLIDO",
                   "Token de refresco inválido o expirado");
            }


            // LOG: Token refrescado
            await bitacoraService.RegistrarAccionComoAsync(
                user.IdUsuario,
                "REFRESH_TOKEN",
                "Token de acceso renovado");

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(Usuario user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GetUserByRefreshTokenAsync(user)
            };
        }

        private async Task<Usuario?> ValidateTokenRefreshTokenAsync(string userId, string refreshToken)
        {
            var user = await context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            return user;
        }

        private async Task<string> GetUserByRefreshTokenAsync(Usuario user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await context.SaveChangesAsync();
            return refreshToken;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private string CreateToken(Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Correo),
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario),
                new Claim(ClaimTypes.Role, user.Rol?.NombreRol ?? "Visitante")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: configuration["AppSettings:Issuer"],
                audience: configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
