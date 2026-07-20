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

        public void SendEmail(EmailDto request)
        {
            // ====== TRAZAS DE DIAGNÓSTICO (NO MODIFICAN LA LÓGICA) ======
            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} EmailService.SendEmail INICIO to={request.To}");
            // ============================================================

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

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.Connect({host}, 587, StartTls) ...");
            var sw = Stopwatch.StartNew();
            try
            {
                smtp.Connect(host, 587, SecureSocketOptions.StartTls);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Connect OK          {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Connect FALLÓ       {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.Authenticate(user) ...");
            sw.Restart();
            try
            {
                smtp.Authenticate(user, pass);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Authenticate OK     {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Authenticate FALLÓ  {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.Send(email) ...");
            sw.Restart();
            try
            {
                smtp.Send(email);
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Send OK             {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                sw.Stop();
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Send FALLÓ          {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"[EMAIL ]   Tipo   : {ex.GetType().FullName}");
                Console.WriteLine($"[EMAIL ]   Mensaje: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[EMAIL ]   Inner  : {ex.InnerException.Message}");
                throw;
            }

            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} >>> smtp.Disconnect(true) ...");
            sw.Restart();
            smtp.Disconnect(true);
            sw.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} <<< smtp.Disconnect OK       {sw.ElapsedMilliseconds} ms");

            swTotal.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} EmailService.SendEmail FIN  total={swTotal.ElapsedMilliseconds} ms");
            Console.WriteLine(new string('-', 80));
        }
    }
}
