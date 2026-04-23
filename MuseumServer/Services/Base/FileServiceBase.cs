using Microsoft.AspNetCore.Http;

namespace MuseumServer.Services.Base
{
    public abstract class FileServiceBase
    {
        // Генерация имени (универсальная)
        protected string GenerateFileName(string extension, string? prefix = null)
        {
            var date = DateTime.UtcNow.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString("N")[..8];

            if (!string.IsNullOrWhiteSpace(prefix))
            {
                return $"{prefix}_{date}_{random}{extension}";
            }

            return $"{date}_{random}{extension}";
        }

        // Проверка уникальности
        protected string EnsureUniqueFileName(string folderPath, string fileName)
        {
            var fullPath = Path.Combine(folderPath, fileName);

            while (File.Exists(fullPath))
            {
                var ext = Path.GetExtension(fileName);
                fileName = GenerateFileName(ext); // без prefix при конфликте
                fullPath = Path.Combine(folderPath, fileName);
            }

            return fileName;
        }

        // Сохранение файла (универсально)
        protected async Task SaveToDiskAsync(string fullPath, IFormFile file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}