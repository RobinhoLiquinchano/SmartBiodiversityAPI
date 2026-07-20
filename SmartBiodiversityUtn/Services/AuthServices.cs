using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.DTOs.Email;
using SmartBiodiversityUtnModels.Entities;
using System.Diagnostics;
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
        private readonly IEmailService _emailService;

        // Tiempo de vida del código de verificación (15 minutos).
        private const int CodigoExpiracionMinutos = 15;

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

            return await GenerateTokenAsync(user);
        }

        // ==================== REGISTRO (con verificación de código) ====================
        public async Task<Usuario?> RegisterAsync(UserDto request)
        {
            // 1. No permitir registrar un correo que ya exista.
            var existingUser = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == request.Correo);

            if (existingUser != null) return null;

            // 2. Validar el código de verificación contra la tabla Tokens.
            var tokenValido = await ObtenerTokenValidoAsync(
                request.Correo,
                request.CodigoVerificacion,
                "Registro");

            if (tokenValido == null) return null;

            // 3. Crear el usuario con el rol por defecto "Usuario Registrado".
            var rolRegistrado = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol == "Usuario Registrado");

            if (rolRegistrado is null) return null;

            var passwordHasher = new PasswordHasher<Usuario>();
            var hashedPassword = passwordHasher.HashPassword(null!, request.Password);

            var usuario = new Usuario
            {
                IdUsuario = "USR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Correo = request.Correo,
                Password = hashedPassword,
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow,
                IntentosFallidos = 0,
                IdRolesU = rolRegistrado.IdRoles
            };

            _context.Usuarios.Add(usuario);

            // 4. Marcar el token como usado.
            tokenValido.Usado = "1";
            tokenValido.IdUsuarioTok = usuario.IdUsuario;
            _context.Tokens.Update(tokenValido);

            await _context.SaveChangesAsync();

            // 5. Bitácora del registro.
            try
            {
                await _bitacoraService.CreateBitacoraAsync(new()
                {
                    IdUsuarioBit = usuario.IdUsuario,
                    AccionBit = "REGISTRO_USUARIO",
                    DetalleBit = $"El usuario {usuario.Correo} completó su registro."
                });
            }
            catch
            {
                // No detenemos el registro si la bitácora falla.
            }

            return usuario;
        }

        // ==================== REFRESH TOKEN ====================
        public async Task<TokenResponseDto?> RefreshTokensAsync(
            RefreshTokenRequestDto requestDto)
        {
            var user = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == requestDto.UserId &&
                    u.RefreshToken == requestDto.RefreshToken);

            if (user is null) return null;
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow) return null;

            return await GenerateTokenAsync(user);
        }

        // ==================== PASSWORD RESET ====================
        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == email);

            if (user == null) return null;

            // 1. Generar código aleatorio.
            var codigo = GenerarCodigoNumerico(6);

            // 2. Construir el correo.
            var emailDto = new EmailDto
            {
                To = email,
                Subject = "🔐 Restablecimiento de contraseña - Smart Biodiversity",
                Body = ConstruirCuerpoCodigoHtml(codigo, "Restablecimiento de contraseña")
            };
            await _emailService.SendEmailAsync(emailDto);

            // 3. Persistir el token.
            var token = new Token
            {
                IdTokens = "TOK-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                IdUsuarioTok = user.IdUsuario,
                CorreoTok = email,
                CodigoTok = HashSha256(codigo),
                TipoTok = "Reset",
                FechaCreacionTok = DateTime.UtcNow,
                FechaExpiracionTok = DateTime.UtcNow.AddMinutes(CodigoExpiracionMinutos),
                Usado = "0"
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return codigo;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // Mantengo la firma original: validación contra la entidad Token por CódigoTok.
            var tokenEntity = await _context.Tokens
                .FirstOrDefaultAsync(t =>
                    t.CodigoTok == token &&
                    t.TipoTok == "Reset" &&
                    t.Usado == "0" &&
                    t.FechaExpiracionTok > DateTime.UtcNow);

            if (tokenEntity == null) return false;

            var user = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == tokenEntity.IdUsuarioTok);
            if (user == null) return false;

            var passwordHasher = new PasswordHasher<Usuario>();
            user.Password = passwordHasher.HashPassword(user, newPassword);
            tokenEntity.Usado = "1";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == userId);
            if (user == null) return false;

            var hasher = new PasswordHasher<Usuario>();
            if (hasher.VerifyHashedPassword(user, user.Password, currentPassword)
                == PasswordVerificationResult.Failed)
                return false;

            user.Password = hasher.HashPassword(user, newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        // ==================== ENVÍO DE CÓDIGO DE REGISTRO ====================
        public async Task<bool> SendVerificationCodeAsync(string email)
        {
            // ====== TRAZAS DE DIAGNÓSTICO (NO MODIFICAN LA LÓGICA) ======
            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} SendVerificationCodeAsync INICIO  email={email}");
            // ============================================================

            // 1. No enviar si el correo ya está registrado.
            var sw = Stopwatch.StartNew();
            bool correoYaRegistrado = await _context.Usuarios.AnyAsync(u => u.Correo == email);
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} SELECT Usuarios.Any           {sw.ElapsedMilliseconds} ms   existe={correoYaRegistrado}");

            if (correoYaRegistrado) return false;

            // 2. Invalidar cualquier token pendiente previo del mismo correo
            //    (mismo flujo que "reenviar").
            sw.Restart();
            var tokensPrevios = await _context.Tokens
                .Where(t =>
                    t.CorreoTok == email &&
                    t.TipoTok == "Registro" &&
                    t.Usado == "0")
                .ToListAsync();
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} SELECT Tokens previos         {sw.ElapsedMilliseconds} ms   count={tokensPrevios.Count}");

            sw.Restart();
            foreach (var t in tokensPrevios)
            {
                t.Usado = "1";
            }
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} Marcar previos como usados  {sw.ElapsedMilliseconds} ms");

            // 3. Generar código nuevo.
            sw.Restart();
            var codigo = GenerarCodigoNumerico(6);
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} Generar código 6 dígitos    {sw.ElapsedMilliseconds} ms   codigo={codigo}");

            // 4. Construir EmailDto y enviar usando IEmailService.
            sw.Restart();
            var emailDto = new EmailDto
            {
                To = email,
                Subject = "🌱 Verificación de registro - Smart Biodiversity",
                Body = ConstruirCuerpoCodigoHtml(
                    codigo,
                    "Verificación de correo electrónico")
            };
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} Construir EmailDto (HTML)    {sw.ElapsedMilliseconds} ms   bytes={emailDto.Body.Length}");

            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} >>> Llamando a IEmailService.SendEmail ...");
            sw.Restart();
            try
            {
                await _emailService.SendEmailAsync(emailDto);
                sw.Stop();
                Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} <<< SendEmail OK              {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception exSmtp)
            {
                sw.Stop();
                Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} <<< SendEmail FALLÓ           {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[TRAZA]   Tipo   : {exSmtp.GetType().FullName}");
                Console.WriteLine($"[TRAZA]   Mensaje: {exSmtp.Message}");
                if (exSmtp.InnerException != null)
                    Console.WriteLine($"[TRAZA]   Inner  : {exSmtp.InnerException.Message}");
                // Si el envío falla, revertimos cualquier cambio previo.
                return false;
            }

            // 5. Persistir el token (con HASH, no el código en plano).
            sw.Restart();
            var token = new Token
            {
                IdTokens = "TOK-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                IdUsuarioTok = null,        // todavía no existe usuario
                CorreoTok = email,
                CodigoTok = HashSha256(codigo),
                TipoTok = "Registro",
                FechaCreacionTok = DateTime.UtcNow,
                FechaExpiracionTok = DateTime.UtcNow.AddMinutes(CodigoExpiracionMinutos),
                Usado = "0"
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();
            sw.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} INSERT Token + SaveChanges   {sw.ElapsedMilliseconds} ms   tokenId={token.IdTokens}");

            swTotal.Stop();
            Console.WriteLine($"[TRAZA] {DateTime.Now:HH:mm:ss.fff} SendVerificationCodeAsync FIN  total={swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine(new string('-', 80));

            return true;
        }

        // ==================== VERIFICACIÓN DE CÓDIGO DE REGISTRO ====================
        public async Task<bool> VerifyRegistrationCodeAsync(string email, string codigo)
        {
            var tokenValido = await ObtenerTokenValidoAsync(email, codigo, "Registro");
            return tokenValido != null;
        }

        // ============================================================
        // =============== MÉTODOS PRIVADOS DE APOYO ==================
        // ============================================================

        /// <summary>
        /// Busca en la tabla Tokens un registro que coincida con el correo,
        /// el tipo, que esté vigente y cuyo HASH coincida con el código recibido.
        /// </summary>
        private async Task<Token?> ObtenerTokenValidoAsync(
            string email,
            string codigo,
            string tipo)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(codigo))
                return null;

            var hash = HashSha256(codigo);

            return await _context.Tokens
                .Where(t =>
                    t.CorreoTok == email &&
                    t.TipoTok == tipo &&
                    t.CodigoTok == hash &&
                    t.Usado == "0" &&
                    t.FechaExpiracionTok > DateTime.UtcNow)
                .OrderByDescending(t => t.FechaCreacionTok)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Genera un código numérico aleatorio de la longitud indicada.
        /// </summary>
        private static string GenerarCodigoNumerico(int longitud)
        {
            var buffer = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);

            // Aseguramos que el número tenga exactamente 'longitud' dígitos.
            var valor = Math.Abs(BitConverter.ToInt32(buffer, 0));
            var codigo = (valor % (int)Math.Pow(10, longitud))
                .ToString()
                .PadLeft(longitud, '0');

            return codigo;
        }

        /// <summary>
        /// Genera el hash SHA-256 en hexadecimal de un texto.
        /// </summary>
        private static string HashSha256(string texto)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(texto));
            return Convert.ToHexString(bytes);
        }

        /// <summary>
        /// Construye el HTML del correo con un diseño amigable.
        /// </summary>
        private static string ConstruirCuerpoCodigoHtml(string codigo, string titulo)
        {
            return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
    <meta charset=""UTF-8"">
    <title>{titulo}</title>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6fa;font-family:Arial,Helvetica,sans-serif;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6fa;padding:32px 0;"">
        <tr>
            <td align=""center"">
                <table role=""presentation"" width=""480"" cellpadding=""0"" cellspacing=""0""
                       style=""background-color:#ffffff;border-radius:12px;
                              box-shadow:0 4px 18px rgba(15,23,42,0.08);
                              overflow:hidden;"">
                    <tr>
                        <td style=""background-color:#059669;padding:24px;text-align:center;color:#ffffff;"">
                            <h1 style=""margin:0;font-size:22px;"">🌱 Smart Biodiversity</h1>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:32px;color:#0f172a;"">
                            <h2 style=""margin:0 0 12px;font-size:20px;color:#065f46;"">
                                {titulo}
                            </h2>
                            <p style=""margin:0 0 16px;line-height:1.5;color:#334155;font-size:15px;"">
                                Hola 👋,<br><br>
                                Usa el siguiente código para continuar con tu proceso.
                                Este código caduca en
                                <strong>{CodigoExpiracionMinutos} minutos</strong>.
                            </p>
                            <div style=""margin:24px 0;text-align:center;"">
                                <span style=""display:inline-block;background-color:#ecfdf5;
                                             color:#065f46;font-size:32px;font-weight:bold;
                                             letter-spacing:8px;padding:16px 28px;
                                             border-radius:10px;border:2px dashed #059669;"">
                                    {codigo}
                                </span>
                            </div>
                            <p style=""margin:0 0 8px;line-height:1.5;color:#334155;font-size:14px;"">
                                Si tú no solicitaste este código, puedes ignorar este mensaje.
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color:#f1f5f9;padding:16px;text-align:center;
                                   color:#64748b;font-size:12px;"">
                            © {DateTime.UtcNow.Year} Smart Biodiversity · Campus El Olivo
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
        }

        // ==================== JWT ====================
        private async Task<TokenResponseDto> GenerateTokenAsync(Usuario user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]!));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUsuario),
                new Claim(ClaimTypes.Name, user.Nombres),
                new Claim(ClaimTypes.Email, user.Correo),
                new Claim(ClaimTypes.Role, user.Rol?.NombreRol ?? "Usuario Registrado")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["AppSettings:Issuer"],
                audience: _configuration["AppSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Refresh token aleatorio
            var refreshToken = Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(64));

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }
    }
}
