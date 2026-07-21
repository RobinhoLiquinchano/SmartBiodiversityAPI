using SmartBiodiversityUtnModels.DTOs.Account;

namespace SmartBiodiversityUtn.Services
{
    public interface IUserService
    {
        Task<UserProfileResponse?> GetProfileAsync(string idUsuario);
        Task<bool> UpdateProfileAsync(string idUsuario, UpdateProfileRequest request);
        Task<IEnumerable<UserListResponse>> GetAllUsersAsync();
        Task<UserListResponse?> GetUserByIdAsync(string idUsuario);
    }
}
