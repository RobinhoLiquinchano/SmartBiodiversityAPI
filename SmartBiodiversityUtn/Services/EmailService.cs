using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SmartBiodiversityUtnModels.DTOs.Email;

namespace SmartBiodiversityUtn.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailDto request)
        {
            // ====== TRAZAS DE DIAGNÓSTICO ======
            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} EmailService.SendEmailAsync INICIO to={request.To}");
            // ====================================

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailUsername"]));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;

            var builder = new BodyBuilder
            {
                HtmlBody = request.Body
            };
            email.Body = builder.ToMessageBody();

            var host = _configuration["EmailHost"];
            var user = _configuration["EmailUsername"];
            var pass = _configuration["EmailPassword"];

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Config leída host={host} user={user} passLen={(pass?.Length ?? 0)}");

            using var smtp = new SmtpClient();

            // ✅ Agregar timeout
            smtp.Timeout = 10000; // 10 segundos

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.ConnectAsync({host}, 587, StartTls) ...");
            var sw = Stopwatch.StartNew();
            try
            {
                // ✅ Cambiar a async
                await smtp.ConnectAsync(host, 587, SecureSocketOptions.StartTls);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.ConnectAsync OK          {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.ConnectAsync FALLÓ       {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.AuthenticateAsync(user) ...");
            sw.Restart();
            try
            {
                // ✅ Cambiar a async
                await smtp.AuthenticateAsync(user, pass);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.AuthenticateAsync OK     {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.AuthenticateAsync FALLÓ  {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.SendAsync(email) ...");
            sw.Restart();
            try
            {
                // ✅ Cambiar a async
                await smtp.SendAsync(email);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.SendAsync OK             {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.SendAsync FALLÓ          {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.DisconnectAsync(true) ...");
            sw.Restart();
            // ✅ Cambiar a async
            await smtp.DisconnectAsync(true);
            sw.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.DisconnectAsync OK       {sw.ElapsedMilliseconds} ms");

            swTotal.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} EmailService.SendEmailAsync FIN  total={swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine(new string('-', 80));
        }
    }
}