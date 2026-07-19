using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class EspecieService(SmartBiodiversityUtnContext context) : IEspecieService
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

            if (especieToDelete is null)
            {
                return false;
            }

            context.Especies.Remove(especieToDelete);
            await context.SaveChangesAsync();

            return true;
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

            if (existingEspecie == null)
            {
                return false;
            }

            existingEspecie.NombreComunEsp = especie.NombreComun ?? existingEspecie.NombreComunEsp;
            existingEspecie.NombreCientificoEsp = especie.NombreCientifico ?? existingEspecie.NombreCientificoEsp;
            existingEspecie.DescripcionEsp = especie.Descripcion ?? existingEspecie.DescripcionEsp;
            existingEspecie.HabitatEsp = especie.Habitat ?? existingEspecie.HabitatEsp;
            existingEspecie.EstadoEsp = especie.EstadoEsp ?? existingEspecie.EstadoEsp;
            existingEspecie.IdCategoriaEsp = especie.IdCategoria ?? existingEspecie.IdCategoriaEsp;

            context.Especies.Update(existingEspecie);
            await context.SaveChangesAsync();

            return true;
        }
    }
}
