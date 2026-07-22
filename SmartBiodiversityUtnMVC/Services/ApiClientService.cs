using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace SmartBiodiversityUtnMVC.Services
{
    public class ApiClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiClientService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void ConfigurarToken()
        {
            var token = _httpContextAccessor.HttpContext?
                .Request.Cookies["AccessToken"];

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            ConfigurarToken();

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string endpoint,
            TRequest data)
        {
            ConfigurarToken();

            var response = await _httpClient.PostAsJsonAsync(endpoint, data);

            if (!response.IsSuccessStatusCode)
                return default;

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task<bool> PostAsync<TRequest>(
            string endpoint,
            TRequest data)
        {
            ConfigurarToken();

            var response = await _httpClient.PostAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PutAsync<TRequest>(
            string endpoint,
            TRequest data)
        {
            ConfigurarToken();

            var response = await _httpClient.PutAsJsonAsync(endpoint, data);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            ConfigurarToken();

            var response = await _httpClient.DeleteAsync(endpoint);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// POST multipart/form-data (para subir archivos a la API reenviando el JWT del admin).
        /// </summary>
        /// <param name="endpoint">Ruta relativa, ej. "api/Multimedias".</param>
        /// <param name="file">Archivo recibido desde el formulario del MVC.</param>
        /// <param name="fileFieldName">Nombre del campo del archivo en la API (en la API es "Archivo").</param>
        /// <param name="fields">Campos de texto adicionales (EspecieId, TipoArchivo, ...).</param>
        public async Task<bool> PostMultipartAsync(
            string endpoint,
            IFormFile file,
            string fileFieldName,
            IEnumerable<KeyValuePair<string, string>>? fields = null)
        {
            ConfigurarToken();

            using var formData = new MultipartFormDataContent();

            // Campos de texto
            if (fields != null)
            {
                foreach (var f in fields)
                    formData.Add(new StringContent(f.Value), f.Key);
            }

            // Archivo
            using var stream = file.OpenReadStream();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            formData.Add(fileContent, fileFieldName, file.FileName);

            var response = await _httpClient.PostAsync(endpoint, formData);
            return response.IsSuccessStatusCode;
        }
    }
}
