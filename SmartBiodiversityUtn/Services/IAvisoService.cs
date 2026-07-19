using SmartBiodiversityUtnModels.DTOs.Aviso;

namespace SmartBiodiversityUtn.Services
{
    public interface IAvisoService
    {
        Task<IEnumerable<AvisoResponse>> GetAllAvisosAsync();
        Task<AvisoResponse?> GetAvisoByIdAsync(string id);
        Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request);
        Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request);
        Task<bool> DeleteAvisoAsync(string id);
        Task<IEnumerable<AvisoResponse>> GetAvisosActivosAsync();
    }
}
