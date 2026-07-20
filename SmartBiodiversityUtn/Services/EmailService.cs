using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SmartBiodiversityUtn.Services
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(
            string toEmail,
            string resetToken
        );

        Task SendVerificationCodeEmailAsync(
            string toEmail,
            string verificationCode
        );
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendPasswordResetEmailAsync(
            string toEmail,
            string resetToken)
        {
            var subject = "Restablecer contraseña - Smart Biodiversity";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2c3e50;'>
                        Restablecimiento de contraseña
                    </h2>

                    <p>
                        Has solicitado restablecer tu contraseña en
                        <strong>Smart Biodiversity</strong>.
                    </p>

                    <div style='
                        background-color: #fef2f2;
                        border: 1px solid #fecaca;
                        padding: 20px;
                        border-radius: 10px;
                        margin: 20px 0;
                        text-align: center;'>

                        <p style='margin: 0; font-size: 16px;'>
                            Tu código de restablecimiento es:
                        </p>

                        <h1 style='
                            color: #dc2626;
                            font-size: 32px;
                            letter-spacing: 5px;
                            margin: 12px 0;'>
                            {resetToken}
                        </h1>
                    </div>

                    <p>
                        <strong>Este código expira en 2 horas.</strong>
                    </p>

                    <p>
                        Si no solicitaste este cambio, puedes ignorar este correo.
                    </p>

                    <hr style='margin: 30px 0;' />

                    <p style='font-size: 12px; color: #7f8c8d;'>
                        Smart Biodiversity UTN - Sistema de Gestión de Biodiversidad
                    </p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendVerificationCodeEmailAsync(
            string toEmail,
            string verificationCode)
        {
            var subject = "Código de verificación - Smart Biodiversity";

            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #15803d;'>
                        Verifica tu correo electrónico
                    </h2>

                    <p>
                        Usa el siguiente código para completar tu registro
                        en <strong>Smart Biodiversity</strong>.
                    </p>

                    <div style='
                        background-color: #dcfce7;
                        border: 1px solid #86efac;
                        padding: 24px;
                        border-radius: 10px;
                        margin: 24px 0;
                        text-align: center;'>

                        <p style='margin: 0; font-size: 15px; color: #166534;'>
                            Tu código de verificación es:
                        </p>

                        <h1 style='
                            color: #166534;
                            font-size: 34px;
                            letter-spacing: 8px;
                            margin: 12px 0 0;'>
                            {verificationCode}
                        </h1>
                    </div>

                    <p>
                        <strong>Este código expira en 10 minutos.</strong>
                    </p>

                    <p>
                        Si no solicitaste este registro, puedes ignorar este correo.
                    </p>

                    <hr style='margin: 30px 0;' />

                    <p style='font-size: 12px; color: #7f8c8d;'>
                        Smart Biodiversity UTN - Sistema de Gestión de Biodiversidad
                    </p>
                </div>";

            await SendEmailAsync(toEmail, subject, body);
        }

        private async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlBody)
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var fromEmail = _configuration["Email:FromEmail"];
            var password = _configuration["Email:Password"];
            var displayName = _configuration["Email:DisplayName"]
                              ?? "Smart Biodiversity";

            var portText = _configuration["Email:Port"] ?? "587";
            var port = int.Parse(portText);

            if (string.IsNullOrWhiteSpace(smtpServer) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new Exception(
                    "La configuración Email está incompleta en appsettings.json."
                );
            }

            var email = new MimeMessage();

            email.From.Add(
                new MailboxAddress(displayName, fromEmail)
            );

            email.To.Add(
                MailboxAddress.Parse(toEmail)
            );

            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlBody
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                smtpServer,
                port,
                SecureSocketOptions.StartTls
            );

            await smtp.AuthenticateAsync(
                fromEmail,
                password
            );

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}