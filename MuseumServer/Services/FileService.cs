using MuseumServer.Services.Base;

namespace MuseumServer.Services
{
    public class FileService : FileServiceBase, IFileService
    {
        private readonly string _rootPath;

        public FileService(IWebHostEnvironment env)
        {
            _rootPath = Path.Combine(env.WebRootPath, "files");
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");

            var folderPath = Path.Combine(_rootPath, subFolder);
            Directory.CreateDirectory(folderPath);

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            var fileName = GenerateFileName(ext);
            fileName = EnsureUniqueFileName(folderPath, fileName);

            var fullPath = Path.Combine(folderPath, fileName);

            await SaveToDiskAsync(fullPath, file);

            return fileName;
        }

        public Task<bool> DeleteFileAsync(string subFolder, string fileName)
        {
            var fullPath = Path.Combine(_rootPath, subFolder, fileName);

            if (!File.Exists(fullPath))
                return Task.FromResult(false);

            File.Delete(fullPath);
            return Task.FromResult(true);
        }
    }
}