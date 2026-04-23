using Microsoft.AspNetCore.Http;

namespace MuseumServer.Services.Base
{
    public abstract class FileServiceBase
    {
        protected string GenerateFileName(string extension, string? prefix = null)
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString("N")[..8];

            return string.IsNullOrWhiteSpace(prefix)
                ? $"{date}_{random}{extension}"
                : $"{prefix}_{date}_{random}{extension}";
        }

        protected string EnsureUniqueFileName(string folderPath, string fileName)
        {
            var fullPath = Path.Combine(folderPath, fileName);

            while (File.Exists(fullPath))
            {
                var ext = Path.GetExtension(fileName);
                fileName = GenerateFileName(ext);
                fullPath = Path.Combine(folderPath, fileName);
            }

            return fileName;
        }

        protected async Task SaveToDiskAsync(string fullPath, IFormFile file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}