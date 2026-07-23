using SmartBiodiversityUtnModels.DTOs.Facultad;

namespace SmartBiodiversityUtn.Services
{
    public interface IFacultadService
    {
        Task<IEnumerable<FacultadResponse>> GetAllFacultadesAsync();
        Task<FacultadResponse?> GetFacultadByIdAsync(string id);
        Task<FacultadEspeciesResponse?> GetEspeciesPorFacultadAsync(string idFacultad);
    }
}
