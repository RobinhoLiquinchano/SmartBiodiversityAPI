using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
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

        // ==================== MÉTODOS CON idUsuario ====================
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

            return MapToResponse(newEspecie, categoria?.NombreCat ?? "Sin categoría");
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
        public async Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync()
        {
            return await _context.Especies
                .Select(e => MapToResponse(e, e.Categoria.NombreCat))
                .ToListAsync();
        }

        public async Task<EspecieResponse?> GetEspecieByIdAsync(string id)
        {
            var especie = await _context.Especies
                .Where(e => e.IdEspecies == id)
                .Select(e => MapToResponse(e, e.Categoria.NombreCat))
                .FirstOrDefaultAsync();

            return especie;
        }

        private EspecieResponse MapToResponse(Especie especie, string nombreCategoria)
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
                FechaRegistroEsp = especie.FechaRegistroEsp
            };
        }
    }
}