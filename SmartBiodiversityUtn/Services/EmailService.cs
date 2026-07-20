using System.Net;
using System.Net.Mail;

namespace SmartBiodiversityUtn.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetToken)
        {
            var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["Email:Port"] ?? "587");
            var fromEmail = _configuration["Email:FromEmail"];
            var password = _configuration["Email:Password"];
            var displayName = _configuration["Email:DisplayName"] ?? "Smart Biodiversity";

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(password))
                throw new Exception("Configuración de email incompleta en appsettings.json");

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, displayName),
                Subject = "Restablecer contraseña - Smart Biodiversity",
                Body = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #2c3e50;'>Restablecimiento de contraseña</h2>
                        <p>Has solicitado restablecer tu contraseña en <strong>Smart Biodiversity</strong>.</p>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <p style='margin: 0; font-size: 16px;'>Tu código de restablecimiento es:</p>
                            <h1 style='color: #e74c3c; font-size: 32px; letter-spacing: 3px; margin: 10px 0;'>{resetToken}</h1>
                        </div>

                        <p><strong>Este código expira en 2 horas.</strong></p>
                        <p>Si no solicitaste este cambio, puedes ignorar este correo.</p>
                        
                        <hr style='margin: 30px 0;'>
                        <p style='font-size: 12px; color: #7f8c8d;'>Smart Biodiversity UTN - Sistema de Gestión de Biodiversidad</p>
                    </div>",
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            await client.SendMailAsync(message);
        }
    }
}