using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using MuseumClient.Models;

namespace MuseumClient.Services
{
    public class ApiService
    {
        private readonly ServerConfig _serverConfig;
        private readonly AuthService _authService;
        private readonly HttpClient _client;

        public ApiService(ServerConfig config, AuthService authService)
        {
            _serverConfig = config;
            _authService = authService;

            _client = new HttpClient();
        }

        private void ApplyHeaders()
        {
            _client.DefaultRequestHeaders.Clear();

            var token = _authService.CurrentToken;
            if (!string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add("token", token);
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            ApplyHeaders();

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<byte[]> GetBytesAsync(string endpoint)
        {
            ApplyHeaders();

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await _client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<T> PostAsync<T>(string endpoint, object payload)
        {
            ApplyHeaders();

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await _client.PostAsJsonAsync(url, payload);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutAsync<T>(string endpoint, object payload)
        {
            ApplyHeaders();

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await _client.PutAsJsonAsync(url, payload);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> DeleteAsync<T>(string endpoint)
        {
            ApplyHeaders();

            var url = $"{_serverConfig.Protocol}://{_serverConfig.Host}:{_serverConfig.Port}/api/{endpoint}";
            var response = await _client.DeleteAsync(url);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }
    }
}