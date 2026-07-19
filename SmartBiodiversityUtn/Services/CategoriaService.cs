using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Categoria;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class CategoriaService : ICategoriaService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IBitacoraService _bitacoraService;

        public CategoriaService(SmartBiodiversityUtnContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria, string idUsuario)
        {
            var newCategoria = new Categoria
            {
                IdCategorias = "CAT-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                NombreCat = categoria.Nombre
            };

            _context.Categorias.Add(newCategoria);
            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "CREAR_CATEGORIA",
                $"Creó la categoría: {categoria.Nombre}");

            return new CategoriaResponse { Id = newCategoria.IdCategorias, Nombre = newCategoria.NombreCat };
        }

        public async Task<bool> DeleteCategoriaAsync(string id, string idUsuario)
        {
            var existing = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategorias == id);
            if (existing is null) return false;

            var nombre = existing.NombreCat;
            _context.Categorias.Remove(existing);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "ELIMINAR_CATEGORIA",
                    $"Eliminó la categoría: {nombre}");
            }

            return result;
        }

        public async Task<IEnumerable<CategoriaResponse>> GetAllCategoriasAsync()
            => await _context.Categorias
                .Select(c => new CategoriaResponse { Id = c.IdCategorias, Nombre = c.NombreCat })
                .ToListAsync();

        public async Task<CategoriaResponse?> GetCategoriaByIdAsync(string id)
        {
            return await _context.Categorias
                .Where(c => c.IdCategorias == id)
                .Select(c => new CategoriaResponse { Id = c.IdCategorias, Nombre = c.NombreCat })
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria, string idUsuario)
        {
            var existing = await _context.Categorias.FirstOrDefaultAsync(c => c.IdCategorias == id);
            if (existing is null) return false;

            existing.NombreCat = categoria.Nombre ?? existing.NombreCat;
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "EDITAR_CATEGORIA",
                    $"Editó la categoría: {existing.NombreCat}");
            }

            return result;
        }

        // Sobrecargas antiguas (compatibilidad)
        public async Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria)
            => await CreateCategoriaAsync(categoria, "SYSTEM");

        public async Task<bool> DeleteCategoriaAsync(string id)
            => await DeleteCategoriaAsync(id, "SYSTEM");

        public async Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria)
            => await UpdateCategoriaAsync(id, categoria, "SYSTEM");
    }
}