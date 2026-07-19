using SmartBiodiversityUtnModels.DTOs.Multimedia;

namespace SmartBiodiversityUtn.Services
{
    public interface IMultimediaService
    {
        Task<IEnumerable<MultimediaResponse>> GetMultimediaByEspecieIdAsync();
        Task<MultimediaResponse> GetMultimediaByEspecieIdAsync(string Id);

        // Con idUsuario (para bitácora)
        Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request, string idUsuario);
        Task<bool> DeleteMultimediaAsync(string id, string idUsuario);

        // Sobrecargas antiguas (compatibilidad)
        Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request);
        Task<bool> DeleteMultimediaAsync(string id);
    }
}