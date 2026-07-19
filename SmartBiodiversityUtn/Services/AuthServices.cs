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
    public class AuthServices : IAuthServices
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBitacoraService _bitacoraService;

        public AuthServices(
            SmartBiodiversityUtnContext context,
            IConfiguration configuration,
            IBitacoraService bitacoraService)
        {
            _context = context;
            _configuration = configuration;
            _bitacoraService = bitacoraService;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginRequest request)
        {
            var user = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == request.Email);

            if (user is null) return null;

            if (new PasswordHasher<Usuario>().VerifyHashedPassword(user, user.Password, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // === BITÁCORA: LOGIN ===
            await BitacoraHelper.RegistrarAccionAsync(
                _bitacoraService,
                user.IdUsuario,
                "LOGIN",
                "Inicio de sesión exitoso");

            return await CreateTokenResponse(user);
        }

        public async Task<Usuario?> RegisterAsync(UserDto request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Correo == request.Correo))
                return null;

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
                IdRolesU = "2" // Visitante por defecto
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            // === BITÁCORA: REGISTRO ===
            await BitacoraHelper.RegistrarAccionAsync(
                _bitacoraService,
                user.IdUsuario,
                "REGISTRO",
                $"Usuario registrado: {user.Correo}");

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto)
        {
            var user = await ValidateTokenRefreshTokenAsync(requestDto.UserId, requestDto.RefreshToken);
            if (user is null) return null;

            // === BITÁCORA: REFRESH TOKEN ===
            await BitacoraHelper.RegistrarAccionAsync(
                _bitacoraService,
                user.IdUsuario,
                "REFRESH_TOKEN",
                "Renovación de token");

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
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return null;

            return user;
        }

        private async Task<string> GetUserByRefreshTokenAsync(Usuario user)
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();
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

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}