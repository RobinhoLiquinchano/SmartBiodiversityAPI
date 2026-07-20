using SmartBiodiversityUtnModels.DTOs.Email;

namespace SmartBiodiversityUtn.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto request);
    }
}
