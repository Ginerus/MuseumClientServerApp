using Microsoft.AspNetCore.Http;
using MuseumServer.Services.Base;

namespace MuseumServer.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder, string? prefix = null);
        Task<bool> DeleteFileAsync(string folder, string fileName);
    }

    public class FileService : FileServiceBase, IFileService
    {
        private readonly string _rootPath;

        public FileService(IWebHostEnvironment env)
        {
            _rootPath = env.WebRootPath;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder, string? prefix = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var folderPath = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(folderPath);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var fileName = GenerateFileName(ext, prefix);
            fileName = EnsureUniqueFileName(folderPath, fileName);

            var fullPath = Path.Combine(folderPath, fileName);

            await SaveToDiskAsync(fullPath, file);

            return fileName;
        }

        public Task<bool> DeleteFileAsync(string folder, string fileName)
        {
            var fullPath = Path.Combine(_rootPath, folder, fileName);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}