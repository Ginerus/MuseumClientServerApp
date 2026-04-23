using Microsoft.AspNetCore.Http;
using MuseumServer.Services.Base;

namespace MuseumServer.Services
{
    public class FileService : FileServiceBase, IFileService
    {
        private readonly string _rootPath;

        public FileService(IWebHostEnvironment env)
        {
            _rootPath = env.WebRootPath;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            folder = folder.Replace("..", "").Trim('/', '\\');

            var folderPath = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(folderPath);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var fileName = GenerateFileName(ext);
            fileName = EnsureUniqueFileName(folderPath, fileName);

            var fullPath = Path.GetFullPath(Path.Combine(folderPath, fileName));

            if (!fullPath.StartsWith(_rootPath))
                throw new Exception("Invalid path");

            await SaveToDiskAsync(fullPath, file);

            return fileName;
        }

        public Task<bool> DeleteFileAsync(string folder, string fileName)
        {
            folder = folder.Replace("..", "").Trim('/', '\\');
            fileName = Path.GetFileName(fileName);

            var fullPath = Path.GetFullPath(Path.Combine(_rootPath, folder, fileName));

            if (!fullPath.StartsWith(_rootPath))
                return Task.FromResult(false);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}