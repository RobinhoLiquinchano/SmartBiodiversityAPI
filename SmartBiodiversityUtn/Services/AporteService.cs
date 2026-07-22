using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtn.Helpers;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;
using SmartBiodiversityUtnModels.DTOs.Bitacora;
using SmartBiodiversityUtnModels.Entities;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SmartBiodiversityUtn.Services
{
    public class AporteService : IAporteService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IBitacoraService _bitacoraService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AporteService(
            SmartBiodiversityUtnContext context,
            IBitacoraService bitacoraService,
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _context = context;
            _bitacoraService = bitacoraService;
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<AporteResponse> CreateAporteAsync(string idUsuario, CreateAporteRequest request, IFormFile? archivo = null)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return null;

            // === LÍMITE DE APORTES POR USUARIO (últimas 24 h) ===
            // Se valida ANTES de subir la foto para no dejar archivos huérfanos en Supabase.
            var limite = LeerLimitePorDia();
            var desde = DateTime.UtcNow.AddHours(-24);
            var hechos = await _context.Aportes
                .CountAsync(a => a.IdUsuarioApo == idUsuario && a.FechaCreacionApo >= desde);
            if (hechos >= limite)
                throw new LimiteAportesExcedidoException(limite);

            // Si viene archivo, se sube a Supabase/Aportes y su URL pública se guarda en RutaArchivoApo.
            // Si no viene archivo, RutaArchivoApo queda null (aporte sin imagen).
            string? rutaArchivo = (archivo != null && archivo.Length > 0)
                ? await SubirAporteAsync(archivo)
                : null;

            var aporte = new Aporte
            {
                IdAporte = "APT-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                IdUsuarioApo = idUsuario,
                TituloApo = request.TituloApo,
                DescripcionApo = request.DescripcionApo,
                RutaArchivoApo = rutaArchivo,
                EstadoApo = EstadoAporte.Pendiente,
                FechaCreacionApo = DateTime.UtcNow
            };

            _context.Aportes.Add(aporte);
            await _context.SaveChangesAsync();

            // === BITÁCORA ===
            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
            {
                IdUsuarioBit = idUsuario,
                AccionBit = "CREAR_APORTE",
                DetalleBit = $"Creó el aporte: {request.TituloApo}"
            });

            return MapToAporteResponse(aporte, usuario);
        }

        // Lee el límite diario de aportes desde appsettings (clave "Aportes:LimitePorDia"). Por defecto 5.
        private int LeerLimitePorDia()
        {
            var raw = _configuration["Aportes:LimitePorDia"];
            return int.TryParse(raw, out var n) && n > 0 ? n : 5;
        }

        // Sube el archivo a Supabase dentro de la carpeta "Aportes" y devuelve la URL pública
        private async Task<string?> SubirAporteAsync(IFormFile archivo)
        {
            var supabaseUrl = _configuration["Supabase:Url"];
            var supabaseKey = _configuration["Supabase:ServiceRoleKey"];
            var bucketName = _configuration["Supabase:BucketName"] ?? "especies-multimedia";

            var fileName = $"{Guid.NewGuid()}_{archivo.FileName}";
            var storagePath = $"{bucketName}/Aportes/{fileName}";
            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{storagePath}";

            using var stream = archivo.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType ?? "application/octet-stream");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);

            var response = await _httpClient.PostAsync(uploadUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir a Supabase: {error}");
            }

            return $"{supabaseUrl}/storage/v1/object/public/{storagePath}";
        }

        public async Task<AporteResponse> GetAporteByIdAsync(string idAporte)
        {
            var aporte = await _context.Aportes
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(a => a.IdAporte == idAporte);

            return aporte == null ? null : MapToAporteResponse(aporte, aporte.Usuario);
        }

        public async Task<IEnumerable<AporteResponse>> GetAllAportesAsync()
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<IEnumerable<AporteResponse>> GetAportesByUsuarioAsync(string idUsuario)
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .Where(a => a.IdUsuarioApo == idUsuario)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<IEnumerable<AporteResponse>> GetAportesByEstadoAsync(EstadoAporte estado)
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .Where(a => a.EstadoApo == estado)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<bool> UpdateAporteAsync(string idAporte, UpdateAporteRequest request)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.TituloApo = request.TituloApo;
            aporte.DescripcionApo = request.DescripcionApo;
            aporte.RutaArchivoApo = request.RutaArchivoApo;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
                {
                    IdUsuarioBit = aporte.IdUsuarioApo,
                    AccionBit = "EDITAR_APORTE",
                    DetalleBit = $"Editó el aporte: {aporte.TituloApo}"
                });
            }

            return result;
        }

        public async Task<bool> DeleteAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo == EstadoAporte.Aprobado) return false;

            var titulo = aporte.TituloApo;
            var usuarioId = aporte.IdUsuarioApo;

            _context.Aportes.Remove(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
                {
                    IdUsuarioBit = usuarioId,
                    AccionBit = "ELIMINAR_APORTE",
                    DetalleBit = $"Eliminó el aporte: {titulo}"
                });
            }

            return result;
        }

        public async Task<bool> ApprovedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Aprobado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
                {
                    IdUsuarioBit = aporte.IdUsuarioApo,
                    AccionBit = "APROBAR_APORTE",
                    DetalleBit = $"Aprobó el aporte: {aporte.TituloApo}"
                });
            }

            return result;
        }

        public async Task<bool> RejectedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Rechazado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
                {
                    IdUsuarioBit = aporte.IdUsuarioApo,
                    AccionBit = "RECHAZAR_APORTE",
                    DetalleBit = $"Rechazó el aporte: {aporte.TituloApo}"
                });
            }

            return result;
        }

        public async Task<int> GetCountAportesByEstadoAsync(EstadoAporte estado)
        {
            return await _context.Aportes
                .Where(a => a.EstadoApo == estado)
                .CountAsync();
        }

        private AporteResponse MapToAporteResponse(Aporte aporte, Usuario usuario)
        {
            return new AporteResponse
            {
                IdAporte = aporte.IdAporte,
                IdUsuarioApo = aporte.IdUsuarioApo,
                TituloApo = aporte.TituloApo,
                DescripcionApo = aporte.DescripcionApo,
                RutaArchivoApo = aporte.RutaArchivoApo,
                EstadoApo = aporte.EstadoApo,
                FechaCreacionApo = aporte.FechaCreacionApo.ToEcuadorTime(),
                FechaAprobacionApo = aporte.FechaAprobacionApo.ToEcuadorTime(),
                NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}",
                CorreoUsuario = usuario.Correo
            };
        }
    }

    /// <summary>Se lanza cuando un usuario supera el límite diario de aportes.</summary>
    public class LimiteAportesExcedidoException : Exception
    {
        public int Limite { get; }
        public LimiteAportesExcedidoException(int limite)
            : base($"Has alcanzado el límite de {limite} aportes por día. Intenta más tarde.")
        {
            Limite = limite;
        }
    }
}
