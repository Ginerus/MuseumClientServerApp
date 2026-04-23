namespace MuseumServer.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file, string folder);
        Task<bool> DeleteFileAsync(string folder, string fileName);
    }
}