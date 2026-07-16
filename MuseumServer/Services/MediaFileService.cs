using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.Models;
using MuseumServer.DTOs;

namespace MuseumServer.Services
{
    public class MediaFileService
    {
        private readonly MuseumContext _context;

        public MediaFileService(MuseumContext context)
        {
            _context = context;
        }

        // Получить все медиафайлы
        public async Task<List<MediaFileResponse>> GetAllAsync()
        {
            return await _context.MediaFiles
                .Select(m => new MediaFileResponse
                {
                    MediaFileId = m.MediaFileId,
                    Title = m.Title,
                    MediaType = m.MediaType,
                    Department = m.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = m.Department.DepartmentId,
                            Name = m.Department.Name
                        }
                        : null
                })
                .ToListAsync();
        }

        // Получить один медиафайл по id
        public async Task<MediaFileFullResponse?> GetAsync(int id)
        {
            return await _context.MediaFiles
                .Where(m => m.MediaFileId == id)
                .Select(m => new MediaFileFullResponse
                {
                    MediaFileId = m.MediaFileId,
                    Title = m.Title,
                    MediaType = m.MediaType,
                    Description = m.Description,

                    Department = m.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = m.Department.DepartmentId,
                            Name = m.Department.Name
                        }
                        : null
                })
                .FirstOrDefaultAsync();
        }

        // Создать медиафайл
        public async Task<MediaFile> CreateAsync(MediaFile media)
        {
            _context.MediaFiles.Add(media);
            await _context.SaveChangesAsync();
            return media;
        }

        // Удалить медиафайл
        public async Task<bool> DeleteAsync(int id)
        {
            var media = await _context.MediaFiles.FindAsync(id);
            if (media == null) return false;

            var fullPath = Path.Combine("wwwroot", media.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));

            // 🧠 базовая папка
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            if (media.MediaType == "image")
            {
                DeleteFileSafe(Path.Combine(webRoot, media.FilePath)); // original

                var thumbPath = media.FilePath.Replace("original", "thumbnails");
                DeleteFileSafe(Path.Combine(webRoot, thumbPath));
            }

            if (media.MediaType == "video")
            {
                // 🎥 original
                DeleteFileSafe(Path.Combine(webRoot, media.FilePath));

                // 🖼 thumbnail
                var thumbPath = media.FilePath
                    .Replace("original", "thumbnails")
                    .Replace(".mp4", ".jpg");

                DeleteFileSafe(Path.Combine(webRoot, thumbPath));

                // 🎞 preview
                var previewPath = media.FilePath
                    .Replace("original", "previews")
                    .Replace(".mp4", "_preview.mp4");

                DeleteFileSafe(Path.Combine(webRoot, previewPath));
            }

            _context.MediaFiles.Remove(media);
            await _context.SaveChangesAsync();

            return true;
        }

        // Получение MediaFiles по id
        public async Task<MediaFile?> GetEntityAsync(int id)
        {
            return await _context.MediaFiles
                .FirstOrDefaultAsync(m => m.MediaFileId == id);
        }

        private void DeleteFileSafe(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // можно логировать, но не валим API
            }
        }
    }
}