using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;

namespace SmartBiodiversityUtn.Services
{
    public interface IAporteService
    {


        Task<AporteResponse> GetAporteByIdAsync(string idAporte);
        Task<IEnumerable<AporteResponse>> GetAllAportesAsync();
        Task<AporteResponse> CreateAporteAsync(string idUsuario, CreateAporteRequest request);

        Task<IEnumerable<AporteResponse>> GetAportesByUsuarioAsync(string idUsuario);

        Task<IEnumerable<AporteResponse>> GetAportesByEstadoAsync(EstadoAporte estado);

        Task<bool> UpdateAporteAsync(string idAporte, UpdateAporteRequest request);

        Task<bool> DeleteAporteAsync(string idAporte);

        Task<bool> ApprovedAporteAsync(string idAporte);

        Task<bool> RejectedAporteAsync(string idAporte);

        Task<int> GetCountAportesByEstadoAsync(EstadoAporte estado);
    }
}
