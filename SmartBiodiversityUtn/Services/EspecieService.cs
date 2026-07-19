using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Services
{
    public class EspecieService(
        SmartBiodiversityUtnContext context,
        IBitacoraService bitacora,
        IHttpContextAccessor httpContextAccessor
        ) : IEspecieService
    {
        public async Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie)
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

            context.Especies.Add(newEspecie);
            await context.SaveChangesAsync();

            var categoria = await context.Categorias.FindAsync(newEspecie.IdCategoriaEsp);

            // LOG: Especie creada
            var currentUserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await bitacora.RegistrarAccionComoAsync(
                currentUserId ?? "SYSTEM",
                "CREAR_ESPECIE",
                $"Especie creada: '{newEspecie.NombreComunEsp}' ({newEspecie.NombreCientificoEsp}) - ID: {newEspecie.IdEspecies} - Categoría: {categoria?.NombreCat}");

            return MapToResponse(newEspecie, categoria?.NombreCat ?? "Sin categoría");
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
                NombreCategoria = nombreCategoria,
                EstadoEsp = especie.EstadoEsp,
                FechaRegistroEsp = especie.FechaRegistroEsp
            };
        }

        public async Task<bool> DeleteEspecieAsync(string id)
        {
            var especieToDelete = await context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (especieToDelete is null) return false;

            var nombreComun = especieToDelete.NombreComunEsp;
            var nombreCientifico = especieToDelete.NombreCientificoEsp;

            context.Especies.Remove(especieToDelete);
            var result = await context.SaveChangesAsync() > 0;

            if (result)
            {
                var currentUserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // LOG: Especie eliminada
                await bitacora.RegistrarAccionComoAsync(
                    currentUserId ?? "SYSTEM",
                    "ELIMINAR_ESPECIE",
                    $"Especie eliminada: '{nombreComun}' ({nombreCientifico}) - ID: {id}");
            }

            return result;
        }

        public async Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync()
            => await context.Especies.Select(e => new EspecieResponse
            {
                IdEspecie = e.IdEspecies,
                NombreComun = e.NombreComunEsp,
                NombreCientifico = e.NombreCientificoEsp,
                Descripcion = e.DescripcionEsp,
                Habitat = e.HabitatEsp,
                EstadoEsp = e.EstadoEsp,
                NombreCategoria = e.Categoria.NombreCat,
                FechaRegistroEsp = e.FechaRegistroEsp
            }).ToListAsync();

        public async Task<EspecieResponse?> GetEspecieByIdAsync(string id)
        {
            var especie = await context.Especies
                .Where(e => e.IdEspecies == id)
                .Select(e => new EspecieResponse
                {
                    IdEspecie = e.IdEspecies,
                    NombreComun = e.NombreComunEsp,
                    NombreCientifico = e.NombreCientificoEsp,
                    Descripcion = e.DescripcionEsp,
                    Habitat = e.HabitatEsp,
                    EstadoEsp = e.EstadoEsp,
                    NombreCategoria = e.Categoria.NombreCat,
                    FechaRegistroEsp = e.FechaRegistroEsp
                }).FirstOrDefaultAsync();
            return especie;
        }

        public async Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie)
        {
            var existingEspecie = await context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (existingEspecie == null) return false;

            var cambios = new List<string>();
            if (existingEspecie.NombreComunEsp != especie.NombreComun) cambios.Add($"Nombre común: '{existingEspecie.NombreComunEsp}' → '{especie.NombreComun}'");
            if (existingEspecie.NombreCientificoEsp != especie.NombreCientifico) cambios.Add($"Nombre científico: '{existingEspecie.NombreCientificoEsp}' → '{especie.NombreCientifico}'");
            if (existingEspecie.DescripcionEsp != especie.Descripcion) cambios.Add("Descripción modificada");
            if (existingEspecie.HabitatEsp != especie.Habitat) cambios.Add("Hábitat modificado");
            if (existingEspecie.EstadoEsp != especie.EstadoEsp) cambios.Add($"Estado: {existingEspecie.EstadoEsp} → {especie.EstadoEsp}");
            if (existingEspecie.IdCategoriaEsp != especie.IdCategoria) cambios.Add($"Categoría: {existingEspecie.IdCategoriaEsp} → {especie.IdCategoria}");

            existingEspecie.NombreComunEsp = especie.NombreComun ?? existingEspecie.NombreComunEsp;
            existingEspecie.NombreCientificoEsp = especie.NombreCientifico ?? existingEspecie.NombreCientificoEsp;
            existingEspecie.DescripcionEsp = especie.Descripcion ?? existingEspecie.DescripcionEsp;
            existingEspecie.HabitatEsp = especie.Habitat ?? existingEspecie.HabitatEsp;
            existingEspecie.EstadoEsp = especie.EstadoEsp ?? existingEspecie.EstadoEsp;
            existingEspecie.IdCategoriaEsp = especie.IdCategoria ?? existingEspecie.IdCategoriaEsp;

            context.Especies.Update(existingEspecie);
            var result = await context.SaveChangesAsync() > 0;

            if (result && cambios.Any())
            {
                var currentUserId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // LOG: Especie actualizada
                await bitacora.RegistrarAccionComoAsync(
                    currentUserId ?? "SYSTEM",
                    "ACTUALIZAR_ESPECIE",
                    $"Especie actualizada (ID: {id}): {string.Join("; ", cambios)}");
            }

            return result;
        }
    }
}
