using SmartBiodiversityUtnModels.DTOs.Multimedia;

namespace SmartBiodiversityUtn.Services
{
    public interface IMultimediaService
    {
        
        Task<IEnumerable<MultimediaResponse>> GetMultimediaByEspecieIdAsync();
        Task<MultimediaResponse> GetMultimediaByEspecieIdAsync(string Id);
        Task<MultimediaResponse> AddMultimediaAsync(CreateMultimediaRequest request);
        Task<bool> DeleteMultimediaAsync(string id);
    }
}
