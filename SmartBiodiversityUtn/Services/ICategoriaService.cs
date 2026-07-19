using SmartBiodiversityUtnModels.DTOs.Categoria;

namespace SmartBiodiversityUtn.Services
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaResponse>> GetAllCategoriasAsync();
        Task<CategoriaResponse?> GetCategoriaByIdAsync(string id);

        // Con idUsuario (para bitácora)
        Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria, string idUsuario);
        Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria, string idUsuario);
        Task<bool> DeleteCategoriaAsync(string id, string idUsuario);

        // Sobrecargas antiguas
        Task<CategoriaResponse> CreateCategoriaAsync(CreateCategoriaRequest categoria);
        Task<bool> UpdateCategoriaAsync(string id, UpdateCategoriaRequest categoria);
        Task<bool> DeleteCategoriaAsync(string id);
    }
}