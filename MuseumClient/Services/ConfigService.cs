using System.IO;
using System.Text.Json;

namespace MuseumClient.Services
{
    public class ConfigService
    {
        public ServerConfig Server { get; private set; }
        public StreamingConfig Streaming { get; private set; }

        public ConfigService()
        {
            var json = File.ReadAllText("AppSettings.json");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Server = new ServerConfig
            {
                LocalUrl = root.GetProperty("Server").GetProperty("LocalUrl").GetString(),
                RemoteUrl = root.GetProperty("Server").GetProperty("RemoteUrl").GetString()
            };

            Streaming = new StreamingConfig
            {
                LocalUrl = root.GetProperty("Streaming").GetProperty("LocalUrl").GetString(),
                RemoteUrl = root.GetProperty("Streaming").GetProperty("RemoteUrl").GetString()
            };
        }
    }

    public class ServerConfig
    {
        public string LocalUrl { get; set; }
        public string RemoteUrl { get; set; }
    }

    public class StreamingConfig
    {
        public string LocalUrl { get; set; }
        public string RemoteUrl { get; set; }
    }
}