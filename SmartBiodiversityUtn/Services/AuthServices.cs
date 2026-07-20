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
using System.Security.Cryptography;


namespace SmartBiodiversityUtn.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IConfiguration _configuration;
        private readonly IBitacoraService _bitacoraService;
        private readonly IEmailService _emailService;

        public AuthServices(
            SmartBiodiversityUtnContext context,
            IConfiguration configuration,
            IBitacoraService bitacoraService,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _bitacoraService = bitacoraService;
            _emailService = emailService;
        }

        // ==================== LOGIN ====================
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

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, user.IdUsuario, "LOGIN", "Inicio de sesión exitoso");
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
                IdRolesU = "2"
            };

            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, user.IdUsuario, "REGISTRO", $"Usuario registrado: {user.Correo}");
            return user;
        }

        // ==================== PASSWORD RESET (usando Token) ====================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == email);
            if (user == null) return null;

            // Invalidar tokens anteriores de tipo Reset
            var oldTokens = await _context.Tokens
                .Where(t => t.IdUsuarioTok == user.IdUsuario && t.TipoTok == "Reset" && t.Usado != "1")
                .ToListAsync();

            foreach (var t in oldTokens) t.Usado = "1";

            var resetToken = new Token
            {
                IdUsuarioTok = user.IdUsuario,
                CodigoTok = Guid.NewGuid().ToString("N"),
                TipoTok = "Reset",
                FechaCreacionTok = DateTime.UtcNow,
                FechaExpiracionTok = DateTime.UtcNow.AddHours(2),
                Usado = "0"
            };

            _context.Tokens.Add(resetToken);
            await _context.SaveChangesAsync();

            // Enviar correo con el token
            try
            {
                await _emailService.SendPasswordResetEmailAsync(user.Correo, resetToken.CodigoTok);
            }
            catch (Exception ex)
            {
                // Mostrar el error real en la consola
                Console.WriteLine("══════════════════════════════════════");
                Console.WriteLine("ERROR AL ENVIAR CORREO:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("══════════════════════════════════════");
            }

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, user.IdUsuario, "SOLICITUD_RESET_PASSWORD", "Solicitó restablecimiento de contraseña");

            return resetToken.CodigoTok;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var tokenEntity = await _context.Tokens
                .FirstOrDefaultAsync(t => t.CodigoTok == token && t.TipoTok == "Reset" && t.Usado != "1");

            if (tokenEntity == null || tokenEntity.FechaExpiracionTok < DateTime.UtcNow)
                return false;

            var user = await _context.Usuarios.FindAsync(tokenEntity.IdUsuarioTok);
            if (user == null) return false;

            // Verificar que no reutilice contraseñas antiguas
            if (await IsPasswordReusedAsync(user, newPassword))
                return false;

            await SavePasswordToHistoryAsync(user);

            var hasher = new PasswordHasher<Usuario>();
            user.Password = hasher.HashPassword(user, newPassword);
            tokenEntity.Usado = "1";

            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, user.IdUsuario, "RESET_PASSWORD", "Restableció su contraseña");

            return true;
        }

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _context.Usuarios.FindAsync(userId);
            if (user == null) return false;

            var hasher = new PasswordHasher<Usuario>();

            if (hasher.VerifyHashedPassword(user, user.Password, currentPassword) != PasswordVerificationResult.Success)
                return false;

            if (await IsPasswordReusedAsync(user, newPassword))
                return false;

            await SavePasswordToHistoryAsync(user);

            user.Password = hasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, userId, "CAMBIO_PASSWORD", "Cambió su contraseña");

            return true;
        }

        // ==================== MÉTODOS PRIVADOS ====================
        private async Task<bool> IsPasswordReusedAsync(Usuario user, string newPassword)
        {
            var hasher = new PasswordHasher<Usuario>();
            var history = await _context.HistorialContra
                .Where(h => h.IdUsuarioHco == user.IdUsuario)
                .OrderByDescending(h => h.FechaHco)
                .Take(5)
                .ToListAsync();

            foreach (var old in history)
            {
                if (hasher.VerifyHashedPassword(user, old.PasswordHashHco, newPassword) == PasswordVerificationResult.Success)
                    return true;
            }
            return false;
        }

        private async Task SavePasswordToHistoryAsync(Usuario user)
        {
            var history = new HistorialContrasena
            {
                IdUsuarioHco = user.IdUsuario,
                PasswordHashHco = user.Password,
                FechaHco = DateTime.UtcNow
            };

            _context.HistorialContra.Add(history);
            await _context.SaveChangesAsync();
        }

        // ==================== REFRESH TOKEN ====================
        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto)
        {
            var user = await ValidateTokenRefreshTokenAsync(requestDto.UserId, requestDto.RefreshToken);
            if (user is null) return null;
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
        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo.ToLower() == email);

            if (usuarioExiste)
                return false;

            // Invalida anteriores códigos de registro del mismo correo.
            var tokensAnteriores = await _context.Tokens
                .Where(t =>
                    t.CorreoTok == email &&
                    t.TipoTok == "Registro" &&
                    t.Usado != "1")
                .ToListAsync();

            foreach (var tokenAnterior in tokensAnteriores)
            {
                tokenAnterior.Usado = "1";
            }

            // Código seguro aleatorio entre 100000 y 999999.
            var codigo = RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();

            var token = new Token
            {
                IdTokens = "REG-" + Guid.NewGuid().ToString("N")
                    .Substring(0, 10).ToUpper(),

                // Aún no hay usuario creado.
                IdUsuarioTok = null,

                CorreoTok = email,

                // Se guarda hash, no el código visible.
                CodigoTok = GenerarHash(codigo),

                TipoTok = "Registro",
                FechaCreacionTok = DateTime.UtcNow,
                FechaExpiracionTok = DateTime.UtcNow.AddMinutes(10),
                Usado = "0"
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationCodeEmailAsync(
                email,
                codigo
            );

            return true;
        }

        public async Task<bool> VerifyRegistrationCodeAsync(
            string email,
            string codigo)
        {
            email = email.Trim().ToLowerInvariant();

            var codigoHash = GenerarHash(codigo.Trim());

            var token = await _context.Tokens
                .Where(t =>
                    t.CorreoTok == email &&
                    t.CodigoTok == codigoHash &&
                    t.TipoTok == "Registro" &&
                    t.Usado != "1" &&
                    t.FechaExpiracionTok >= DateTime.UtcNow)
                .OrderByDescending(t => t.FechaCreacionTok)
                .FirstOrDefaultAsync();

            return token != null;
        }

        private static string GenerarHash(string texto)
        {
            using var sha256 = SHA256.Create();

            var bytes = sha256.ComputeHash(
                Encoding.UTF8.GetBytes(texto)
            );

            return Convert.ToHexString(bytes);
        }
    }
}