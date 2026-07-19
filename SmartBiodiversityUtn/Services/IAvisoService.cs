using SmartBiodiversityUtnModels.DTOs.Aviso;

namespace SmartBiodiversityUtn.Services
{
    public interface IAvisoService
    {
        Task<IEnumerable<AvisoResponse>> GetAllAvisosAsync();
        Task<AvisoResponse?> GetAvisoByIdAsync(string id);
        Task<IEnumerable<AvisoResponse>> GetAvisosActivosAsync();

        // Con idUsuario
        Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request, string idUsuario);
        Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request, string idUsuario);
        Task<bool> DeleteAvisoAsync(string id, string idUsuario);

        // Sobrecargas antiguas
        Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request);
        Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request);
        Task<bool> DeleteAvisoAsync(string id);
    }
}