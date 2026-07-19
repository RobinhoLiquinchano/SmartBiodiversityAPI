using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public interface IAuthServices
    {
        Task<Usuario?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(LoginRequest request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto requestDto);
    }
}
