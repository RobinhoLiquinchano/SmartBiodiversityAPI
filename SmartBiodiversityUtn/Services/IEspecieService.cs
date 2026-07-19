using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;

namespace SmartBiodiversityUtn.Services
{
    public interface IEspecieService
    {
        // Métodos públicos (sin autenticación)
        Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync();
        Task<EspecieResponse?> GetEspecieByIdAsync(string id);

        Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie, string idUsuario);
        Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie, string idUsuario);
        Task<bool> DeleteEspecieAsync(string id, string idUsuario);

        Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie);
        Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie);
        Task<bool> DeleteEspecieAsync(string id);
    }
}