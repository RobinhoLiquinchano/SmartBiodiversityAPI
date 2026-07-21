using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SmartBiodiversityUtnModels.DTOs.Email;

namespace SmartBiodiversityUtn.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public EmailService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(EmailDto request)
        {
            var swTotal = Stopwatch.StartNew();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Brevo.SendAsync INICIO to={request.To}");

            var apiKey = _configuration["BrevoApiKey"]
                         ?? throw new InvalidOperationException("Falta configurar BrevoApiKey.");
            var senderEmail = _configuration["BrevoSenderEmail"] ?? "robinholiquinchano@gmail.com";
            var senderName = _configuration["BrevoSenderName"] ?? "Smart Biodiversity";

            // Brevo usa HTTPS (443) → Render NO lo bloquea ✅
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");

            httpRequest.Headers.TryAddWithoutValidation("api-key", apiKey);
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var payload = new
            {
                sender = new { name = senderName, email = senderEmail },
                to = new[] { new { email = request.To } },
                subject = request.Subject,
                htmlContent = request.Body
            };

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.SendAsync(httpRequest);
            sw.Stop();
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Brevo FALLÓ {(int)response.StatusCode} {sw.ElapsedMilliseconds} ms -> {responseBody}");
                throw new InvalidOperationException(
                    $"Error enviando correo con Brevo ({(int)response.StatusCode}): {responseBody}");
            }

            swTotal.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Brevo OK {sw.ElapsedMilliseconds} ms  total={swTotal.ElapsedMilliseconds} ms");
        }
    }
}