using Microsoft.AspNetCore.Http;

namespace MuseumServer.Services.Base
{
    public abstract class FileServiceBase
    {
        protected string GenerateFileName(string extension, string prefix = "exh")
        {
            var date = DateTime.UtcNow.ToString("ddMMyyyy");
            var random = Guid.NewGuid().ToString("N")[..8];

            return $"{prefix}{date}{random}{extension}";
        }

        protected string EnsureUniqueFileName(string fullFolderPath, string fileName)
        {
            var fullPath = Path.Combine(fullFolderPath, fileName);

            while (File.Exists(fullPath))
            {
                var ext = Path.GetExtension(fileName);
                fileName = GenerateFileName(ext);
                fullPath = Path.Combine(fullFolderPath, fileName);
            }

            return fileName;
        }

        protected async Task SaveToDiskAsync(string fullPath, IFormFile file)
        {
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}