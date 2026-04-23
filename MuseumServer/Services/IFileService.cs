namespace MuseumServer.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder);
        Task<bool> DeleteFileAsync(string subFolder, string fileName);
    }
}