using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Multimedia;
using SmartBiodiversityUtnModels.Entities;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Services
{
    public class MultimediaService(
            SmartBiodiversityUtnContext _context,
            IConfiguration _configuration,
            HttpClient _httpClient,
            IBitacoraService _bitacora,
        IHttpContextAccessor _httpContextAccessor) : IMultimediaService
    {

        public async Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request)
        {
            // Validar que la especie exista y traer su categoría
            var especie = await _context.Especies
                .Include(e => e.Categoria)
                .FirstOrDefaultAsync(e => e.IdEspecies == request.EspecieId);

            if (especie is null)
                throw new Exception($"La especie con ID {request.EspecieId} no existe.");

            // === CONFIGURACIÓN DE SUPABASE ===
            var supabaseUrl = _configuration["Supabase:Url"];
            var supabaseKey = _configuration["Supabase:ServiceRoleKey"];
            var bucketName = _configuration["Supabase:BucketName"] ?? "especies";

            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
                throw new Exception("Configuración de Supabase incompleta (Url o ServiceRoleKey).");

            // === CARPETA SEGÚN CATEGORÍA (Fauna, Flora, etc.) ===
            var carpetaCategoria = ObtenerCarpetaPorCategoria(especie.Categoria?.NombreCat);

            // === SUBIR ARCHIVO A SUPABASE ===
            var fileName = $"{Guid.NewGuid()}_{request.Archivo.FileName}";
            var storagePath = $"{bucketName}/{carpetaCategoria}/{fileName}";

            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{storagePath}";

            using var stream = request.Archivo.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(request.Archivo.ContentType ?? "application/octet-stream");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", supabaseKey);

            var response = await _httpClient.PostAsync(uploadUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir archivo a Supabase: {error}");
            }

            // URL pública del archivo
            var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{storagePath}";

            // === GUARDAR EN BASE DE DATOS ===
            var multimedia = new Multimedia
            {
                IdEspeciesMul = request.EspecieId,
                TipoArchivoMul = request.TipoArchivo,
                RutaArchivoMul = publicUrl,
                FechaMul = DateTime.UtcNow
            };

            _context.Multimedia.Add(multimedia);
            await _context.SaveChangesAsync();

            // LOG: Multimedia subida
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            await _bitacora.RegistrarAccionComoAsync(
                currentUserId ?? "SYSTEM",
                "SUBIR_MULTIMEDIA",
                $"Archivo subido: {request.Archivo.FileName} ({request.TipoArchivo}) - Especie: {especie?.NombreComunEsp} (ID: {request.EspecieId}) - URL: {publicUrl}");

            return new MultimediaResponse
            {
                IdMultimedia = multimedia.IdMultimedia,
                EspecieId = multimedia.IdEspeciesMul,
                TipoArchivo = multimedia.TipoArchivoMul,
                RutaArchivo = multimedia.RutaArchivoMul,
                Fecha = multimedia.FechaMul
            };

        }

        private static string ObtenerCarpetaPorCategoria(string? nombreCategoria)
        {
            if (string.IsNullOrWhiteSpace(nombreCategoria))
                return "Otros";

            var nombre = nombreCategoria.Trim().ToLowerInvariant();

            return nombre switch
            {
                "fauna" => "Fauna",
                "flora" => "Flora",
                _ => nombreCategoria.Trim() // usa el nombre tal cual si es otra categoría
            };
        }

        public async Task<MultimediaResponse> GetMultimediaByEspecieIdAsync(string especieId)
        {
            var mutimedia = await _context.Multimedia
                .Where(m => m.IdEspeciesMul == especieId)
                .Select(m => new MultimediaResponse
                {
                    IdMultimedia = m.IdMultimedia,
                    EspecieId = m.IdEspeciesMul,
                    TipoArchivo = m.TipoArchivoMul,
                    RutaArchivo = m.RutaArchivoMul,
                    Fecha = m.FechaMul
                }).FirstOrDefaultAsync();

            return mutimedia;
        }

        public async Task<bool> DeleteMultimediaAsync(string id)
        {
            var multimedia = await _context.Multimedia.FindAsync(id);
            if (multimedia is null)
                return false;

            // TODO: Opcional - Eliminar archivo físico de Supabase Storage

            _context.Multimedia.Remove(multimedia);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MultimediaResponse>> GetMultimediaByEspecieIdAsync()
            => await _context.Multimedia
            .Select(m => new MultimediaResponse
            {
                IdMultimedia = m.IdMultimedia,
                EspecieId = m.IdEspeciesMul,
                TipoArchivo = m.TipoArchivoMul,
                RutaArchivo = m.RutaArchivoMul,
                Fecha = m.FechaMul
            }).ToListAsync();
    }
}
