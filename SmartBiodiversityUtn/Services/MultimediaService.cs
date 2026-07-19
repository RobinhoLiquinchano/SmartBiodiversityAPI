using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Multimedia;
using SmartBiodiversityUtnModels.Entities;
using System.Net.Http.Headers;

namespace SmartBiodiversityUtn.Services
{
    public class MultimediaService : IMultimediaService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IBitacoraService _bitacoraService;

        public MultimediaService(
            SmartBiodiversityUtnContext context,
            IConfiguration configuration,
            HttpClient httpClient,
            IBitacoraService bitacoraService)
        {
            _context = context;
            _configuration = configuration;
            _httpClient = httpClient;
            _bitacoraService = bitacoraService;
        }

        public async Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request, string idUsuario)
        {
            var especie = await _context.Especies
                .Include(e => e.Categoria)
                .FirstOrDefaultAsync(e => e.IdEspecies == request.EspecieId);

            if (especie is null)
                throw new Exception($"La especie con ID {request.EspecieId} no existe.");

            var supabaseUrl = _configuration["Supabase:Url"];
            var supabaseKey = _configuration["Supabase:ServiceRoleKey"];
            var bucketName = _configuration["Supabase:BucketName"] ?? "especies-multimedia";

            var carpeta = ObtenerCarpetaPorCategoria(especie.Categoria?.NombreCat);
            var fileName = $"{Guid.NewGuid()}_{request.Archivo.FileName}";
            var storagePath = $"{bucketName}/{carpeta}/{fileName}";
            var uploadUrl = $"{supabaseUrl}/storage/v1/object/{storagePath}";

            using var stream = request.Archivo.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(request.Archivo.ContentType ?? "application/octet-stream");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", supabaseKey);

            var response = await _httpClient.PostAsync(uploadUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir a Supabase: {error}");
            }

            var publicUrl = $"{supabaseUrl}/storage/v1/object/public/{storagePath}";

            var multimedia = new Multimedia
            {
                IdMultimedia = "MUL-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                IdEspeciesMul = request.EspecieId,
                TipoArchivoMul = request.TipoArchivo,
                RutaArchivoMul = publicUrl,
                FechaMul = DateTime.UtcNow
            };

            _context.Multimedia.Add(multimedia);
            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "SUBIR_MULTIMEDIA",
                $"Subió multimedia para: {especie.NombreComunEsp}");

            return new MultimediaResponse
            {
                IdMultimedia = multimedia.IdMultimedia,
                EspecieId = multimedia.IdEspeciesMul,
                TipoArchivo = multimedia.TipoArchivoMul,
                RutaArchivo = multimedia.RutaArchivoMul,
                Fecha = multimedia.FechaMul
            };
        }

        public async Task<bool> DeleteMultimediaAsync(string id, string idUsuario)
        {
            var multimedia = await _context.Multimedia.FindAsync(id);
            if (multimedia is null) return false;

            _context.Multimedia.Remove(multimedia);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "ELIMINAR_MULTIMEDIA",
                    $"Eliminó multimedia ID: {id}");
            }

            return result;
        }

        public async Task<MultimediaResponse> GetMultimediaByEspecieIdAsync(string especieId)
        {
            return await _context.Multimedia
                .Where(m => m.IdEspeciesMul == especieId)
                .Select(m => new MultimediaResponse
                {
                    IdMultimedia = m.IdMultimedia,
                    EspecieId = m.IdEspeciesMul,
                    TipoArchivo = m.TipoArchivoMul,
                    RutaArchivo = m.RutaArchivoMul,
                    Fecha = m.FechaMul
                }).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<MultimediaResponse>> GetMultimediaByEspecieIdAsync()
        {
            return await _context.Multimedia
                .Select(m => new MultimediaResponse
                {
                    IdMultimedia = m.IdMultimedia,
                    EspecieId = m.IdEspeciesMul,
                    TipoArchivo = m.TipoArchivoMul,
                    RutaArchivo = m.RutaArchivoMul,
                    Fecha = m.FechaMul
                }).ToListAsync();
        }

        private static string ObtenerCarpetaPorCategoria(string? nombreCategoria)
        {
            if (string.IsNullOrWhiteSpace(nombreCategoria)) return "Otros";
            return nombreCategoria.Trim().ToLower() switch
            {
                "fauna" => "Fauna",
                "flora" => "Flora",
                _ => nombreCategoria.Trim()
            };
        }

        // Sobrecargas antiguas (compatibilidad)
        public async Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request)
            => await AddMultimediaAsync(request, "SYSTEM");

        public async Task<bool> DeleteMultimediaAsync(string id)
            => await DeleteMultimediaAsync(id, "SYSTEM");
    }
}