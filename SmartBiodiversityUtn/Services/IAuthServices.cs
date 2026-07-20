using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public interface IAuthServices
    {
        Task<Usuario?> RegisterAsync(UserDto request);

        Task<TokenResponseDto?> LoginAsync(LoginRequest request);

        Task<TokenResponseDto?> RefreshTokensAsync(
            RefreshTokenRequestDto requestDto
        );

        Task<string?> GeneratePasswordResetTokenAsync(string email);

        Task<bool> ResetPasswordAsync(string token, string newPassword);

        Task<bool> ChangePasswordAsync(
            string userId,
            string currentPassword,
            string newPassword
        );

        Task<bool> SendVerificationCodeAsync(string email);

        Task<bool> VerifyRegistrationCodeAsync(
            string email,
            string codigo
        );
    }
}