using System.Net.Http.Headers;
using System.Net.Http.Json;

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
    }
}