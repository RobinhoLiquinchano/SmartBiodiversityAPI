using SmartBiodiversityUtnModels.DTOs.Bitacora;

namespace SmartBiodiversityUtn.Services
{
    public interface IBitacoraService
    {
        Task<IEnumerable<BitacoraResponse>> GetAllBitacorasAsync();
        Task<IEnumerable<BitacoraResponse>> GetBitacorasByUsuarioAsync(string idUsuario);
        Task<BitacoraResponse> CreateBitacoraAsync(CreateBitacoraRequest request);
    }
}
