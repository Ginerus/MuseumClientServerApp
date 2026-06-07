using MuseumClient.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace MuseumClient.Services
{
    public class AuthService
    {
        private static AuthService? _instance;

        public static AuthService Instance(ServerConfig? config = null)
        {
            if (_instance == null)
            {
                if (config == null)
                    throw new Exception("AuthService не инициализирован!");

                _instance = new AuthService(config);
            }

            return _instance;
        }

        private readonly ServerConfig _serverConfig;

        private string _token;
        private string _baseUrl;

        private AuthService(ServerConfig config)
        {
            _serverConfig = config;
        }

        public string CurrentToken => _token;
        public string BaseUrl => _baseUrl;

        private async Task<string> GetWorkingUrlAsync()
        {
            using var client = new HttpClient();

            client.Timeout = TimeSpan.FromSeconds(2);

            // 1. пробуем локальный сервер
            try
            {
                var localTestUrl = $"{_serverConfig.LocalUrl}/api/Session/register";

                var response = await client.GetAsync(localTestUrl);

                System.Diagnostics.Debug.WriteLine("LOCAL TEST: " + localTestUrl);

                if (response.IsSuccessStatusCode)
                    return _serverConfig.LocalUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("LOCAL FAIL: " + ex.Message);
            }

            // 2. fallback на удалённый
            System.Diagnostics.Debug.WriteLine("USING REMOTE: " + _serverConfig.RemoteUrl);

            return _serverConfig.RemoteUrl;
        }

        public async Task<AuthResult> RegisterAsync(string userType, string password)
        {
            _baseUrl = await GetWorkingUrlAsync();

            var url = $"{_baseUrl}/api/Session/register";

            System.Diagnostics.Debug.WriteLine("REGISTER URL: " + url);

            var payload = new
            {
                userType,
                password
            };

            using var client = new HttpClient();

            try
            {
                var response = await client.PostAsJsonAsync(url, payload);

                var body = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("RESPONSE: " + body);

                if (!response.IsSuccessStatusCode)
                    return AuthResult.InvalidCredentials;

                var json = await response.Content.ReadFromJsonAsync<UserSession>();

                if (json?.status == "ok")
                {
                    _token = json.token;
                    return AuthResult.Success;
                }

                return AuthResult.InvalidCredentials;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine("REGISTER ERROR: " + ex);

                return AuthResult.ServerUnavailable;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine("TIMEOUT: " + ex);

                return AuthResult.ServerUnavailable;
            }
        }
    }
}