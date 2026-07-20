using SmartBiodiversityUtnModels.DTOs.Email;

namespace SmartBiodiversityUtn.Services
{
    public interface IEmailService
    {
        void SendEmail(EmailDto request);
    }
}
