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
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Resend.SendAsync INICIO to={request.To}");

            // En Render la variable de entorno se llama ResendApiKey
            var apiKey = _configuration["ResendApiKey"]
                         ?? throw new InvalidOperationException("Falta configurar ResendApiKey.");
            var from = _configuration["ResendFrom"]
                       ?? "Smart Biodiversity <onboarding@resend.dev>";

            // Resend usa HTTPS (puerto 443) → NO lo bloquea Render ✅
            using var httpRequest = new HttpRequestMessage(
                HttpMethod.Post, "https://api.resend.com/emails");

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from,
                to = new[] { request.To },
                subject = request.Subject,
                html = request.Body
            };

            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var sw = Stopwatch.StartNew();
            var response = await _httpClient.SendAsync(httpRequest);
            sw.Stop();

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Resend FALLÓ {(int)response.StatusCode} {sw.ElapsedMilliseconds} ms -> {responseBody}");
                throw new InvalidOperationException(
                    $"Error enviando correo con Resend ({(int)response.StatusCode}): {responseBody}");
            }

            swTotal.Stop();
            Console.WriteLine($"[EMAIL ] {DateTime.Now:HH:mm:ss.fff} Resend OK {sw.ElapsedMilliseconds} ms  total={swTotal.ElapsedMilliseconds} ms");
        }
    }
}