using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Categoria;
using SmartBiodiversityUtnModels.DTOs.Especie;

namespace SmartBiodiversityUtn.Services
{
    public interface IEspecieService
    {
        Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync();
        Task<EspecieResponse?> GetEspecieByIdAsync(string id);
        Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie);
        Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie);
        Task<bool> DeleteEspecieAsync(string id);
    }
}
