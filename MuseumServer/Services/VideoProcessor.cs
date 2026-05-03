using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MuseumServer.Services
{
    public class VideoProcessor
    {
        private readonly string _rootPath;

        public VideoProcessor(IWebHostEnvironment env)
        {
            _rootPath = env.WebRootPath;
        }

        // Создание thumbnail (кадр из видео)
        public async Task CreateThumbnailAsync(
            string inputPath,
            string outputPath,
            int width = 300)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var args =
                $"-y -ss 00:00:02 -i \"{inputPath}\" -vframes 1 " +
                $"-vf scale={width}:-1 \"{outputPath}\"";

            await RunFFmpegAsync(args);
        }

        // Создание preview-видео
        public async Task CreatePreviewAsync(
            string inputPath,
            string outputPath,
            int width = 320,
            int durationSeconds = 4)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            var args =
                $"-y -ss 00:00:02 -i \"{inputPath}\" -t {durationSeconds} " +
                $"-vf scale={width}:-1 -an -c:v libx264 -preset veryfast " +
                $"-crf 28 \"{outputPath}\"";

            await RunFFmpegAsync(args);
        }

        private async Task RunFFmpegAsync(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg", // должен быть в PATH
                    Arguments = arguments,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg error: {error}");
            }
        }

        // Определение пути хранения ffmpeg для конкретных ОС
        private string GetFFmpegPath()
        {
            var basePath = AppContext.BaseDirectory;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Path.Combine(basePath, "Tools", "ffmpeg", "win", "ffmpeg.exe");
            }

            return Path.Combine(basePath, "Tools", "ffmpeg", "linux", "ffmpeg");
        }
    }
}