using SmartBiodiversityUtnModels.DTOs.Categoria;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaResponse>> GetAllCategoriasAsync();
        Task<CategoriaResponse?> GetCategoriaByIdAsync(string id);
        Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria);
        Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria);
        Task<bool> DeleteCategoriaAsync(string id);
    }
}
