using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Categoria;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Services
{
    public class CategoriaService(
        SmartBiodiversityUtnContext context) : ICategoriaService
    {
        public async Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria)
        {
            var newCategoria = new Categoria
            {
                IdCategorias = "CAT-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                NombreCat = categoria.Nombre
            };

            context.Categorias.Add(newCategoria);
            await context.SaveChangesAsync();

            return new CategoriaResponse
            {
                Id = newCategoria.IdCategorias,
                Nombre = newCategoria.NombreCat
            };
        }

        public async Task<bool> DeleteCategoriaAsync(string id)
        {
            var existingCategoria = await context.Categorias.FirstOrDefaultAsync(c => c.IdCategorias == id);

            if (existingCategoria is null)
            {
                return false;
            }
            var nombre = existingCategoria.NombreCat;

            context.Categorias.Remove(existingCategoria);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CategoriaResponse>> GetAllCategoriasAsync()
        => await context.Categorias.Select(c => new CategoriaResponse
        {
            Id = c.IdCategorias,
            Nombre = c.NombreCat
        }).ToListAsync();

        public async Task<CategoriaResponse?> GetCategoriaByIdAsync(string id)
        {
            var result = await context.Categorias
                .Where(c => c.IdCategorias == id)
                .Select(c => new CategoriaResponse
                {
                    Id = c.IdCategorias,
                    Nombre = c.NombreCat
                }).FirstOrDefaultAsync();

            return result;
        }

        public async Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria)
        {
            var existingCategoria = await context.Categorias.FirstOrDefaultAsync(c => c.IdCategorias == id);

            if (existingCategoria is null)
            {
                return false;
            }

            existingCategoria.NombreCat = categoria.Nombre ?? existingCategoria.NombreCat;

            var nombreAnterior = existingCategoria.NombreCat;
            existingCategoria.NombreCat = categoria.Nombre ?? existingCategoria.NombreCat;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
