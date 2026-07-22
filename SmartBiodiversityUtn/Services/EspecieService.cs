using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtn.Helpers;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class EspecieService : IEspecieService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IBitacoraService _bitacoraService;

        public EspecieService(SmartBiodiversityUtnContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie, string idUsuario)
        {
            var newEspecie = new Especie
            {
                IdEspecies = "ESP-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                IdCategoriaEsp = especie.CategoriaId,
                NombreComunEsp = especie.NombreComun,
                NombreCientificoEsp = especie.NombreCientifico,
                DescripcionEsp = especie.Descripcion,
                HabitatEsp = especie.Habitat,
                EstadoEsp = EstadoEspecie.Activo,
                FechaRegistroEsp = DateTime.UtcNow
            };

            _context.Especies.Add(newEspecie);
            await _context.SaveChangesAsync();

            var categoria = await _context.Categorias.FindAsync(newEspecie.IdCategoriaEsp);

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "CREAR_ESPECIE",
                $"Creó la especie: {especie.NombreComun}");

            // Al crear aún no hay imagen
            return MapToResponse(newEspecie, categoria?.NombreCat ?? "Sin categoría", null);
        }

        public async Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie, string idUsuario)
        {
            var existing = await _context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (existing == null) return false;

            existing.NombreComunEsp = especie.NombreComun ?? existing.NombreComunEsp;
            existing.NombreCientificoEsp = especie.NombreCientifico ?? existing.NombreCientificoEsp;
            existing.DescripcionEsp = especie.Descripcion ?? existing.DescripcionEsp;
            existing.HabitatEsp = especie.Habitat ?? existing.HabitatEsp;
            existing.EstadoEsp = especie.EstadoEsp ?? existing.EstadoEsp;
            existing.IdCategoriaEsp = especie.IdCategoria ?? existing.IdCategoriaEsp;

            _context.Especies.Update(existing);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "EDITAR_ESPECIE",
                    $"Editó la especie: {existing.NombreComunEsp}");
            }

            return result;
        }

        public async Task<bool> DeleteEspecieAsync(string id, string idUsuario)
        {
            var especie = await _context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (especie == null) return false;

            var nombre = especie.NombreComunEsp;
            _context.Especies.Remove(especie);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "ELIMINAR_ESPECIE",
                    $"Eliminó la especie: {nombre}");
            }

            return result;
        }

        // ==================== Sobrecargas antiguas (compatibilidad) ====================
        public async Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie)
            => await AddEspecieAsync(especie, "SYSTEM");

        public async Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie)
            => await UpdateEspecieAsync(id, especie, "SYSTEM");

        public async Task<bool> DeleteEspecieAsync(string id)
            => await DeleteEspecieAsync(id, "SYSTEM");

        // ==================== Métodos de lectura ====================
        // Traen la URL de la imagen (la más reciente) de cada especie en la misma consulta,
        // sin N+1: una sola lectura de Multimedia agrupada en memoria.
        public async Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync()
        {
            var especies = await _context.Especies
                .Include(e => e.Categoria)
                .ToListAsync();

            var dictImagenes = await MapaImagenesPorEspecieAsync();

            return especies
                .Select(e => MapToResponse(
                    e,
                    e.Categoria?.NombreCat ?? "Sin categoría",
                    dictImagenes.TryGetValue(e.IdEspecies, out var url) ? url : null))
                .ToList();
        }

        public async Task<EspecieResponse?> GetEspecieByIdAsync(string id)
        {
            var especie = await _context.Especies
                .Include(e => e.Categoria)
                .FirstOrDefaultAsync(e => e.IdEspecies == id);

            if (especie == null) return null;

            var url = await _context.Multimedia
                .Where(m => m.IdEspeciesMul == id && !string.IsNullOrEmpty(m.RutaArchivoMul))
                .OrderByDescending(m => m.FechaMul)
                .Select(m => m.RutaArchivoMul)
                .FirstOrDefaultAsync();

            return MapToResponse(especie, especie.Categoria?.NombreCat ?? "Sin categoría", url);
        }

        /// <summary>
        /// IdEspecie -> URL de la imagen más reciente (solo las que tengan URL).
        /// </summary>
        private async Task<Dictionary<string, string>> MapaImagenesPorEspecieAsync()
        {
            var multimedias = await _context.Multimedia
                .Where(m => !string.IsNullOrEmpty(m.RutaArchivoMul))
                .ToListAsync();

            return multimedias
                .GroupBy(m => m.IdEspeciesMul)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(m => m.FechaMul).First().RutaArchivoMul);
        }

        // static: no captura estado de la instancia (EF10 friendly)
        private static EspecieResponse MapToResponse(Especie especie, string nombreCategoria, string? imagenUrl)
        {
            return new EspecieResponse
            {
                IdEspecie = especie.IdEspecies,
                NombreComun = especie.NombreComunEsp,
                NombreCientifico = especie.NombreCientificoEsp,
                Descripcion = especie.DescripcionEsp,
                Habitat = especie.HabitatEsp,
                EstadoEsp = especie.EstadoEsp,
                NombreCategoria = nombreCategoria,
                FechaRegistroEsp = especie.FechaRegistroEsp.ToEcuadorTime(),
                ImagenUrl = imagenUrl
            };
        }
    }
}
