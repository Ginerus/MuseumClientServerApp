using MuseumServer.Data;
using MuseumServer.Models;
using Microsoft.EntityFrameworkCore;
using MuseumServer.DTOs;

namespace MuseumServer.Services
{
    public class ExhibitService
    {
        private readonly MuseumContext _context;
        private readonly IFileService _fileService;
        private readonly ImageProcessor _imageProcessor;

        public ExhibitService(
            MuseumContext context,
            IFileService fileService,
            ImageProcessor imageProcessor)
        {
            _context = context;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
        }

        public async Task<List<ExhibitWithDepartmentResponse>> GetAllExhibitsAsync()
        {
            return await _context.Exhibits
                .Select(e => new ExhibitWithDepartmentResponse
                {
                    ExhibitId = e.ExhibitId,
                    Name = e.Name,

                    Department = e.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = e.Department.DepartmentId,
                            Name = e.Department.Name
                        }
                        : null
                })
                .ToListAsync();
        }

        public async Task<ExhibitFullResponse?> GetExhibitAsync(int id)
        {
            return await _context.Exhibits
                .Where(e => e.ExhibitId == id)
                .Select(e => new ExhibitFullResponse
                {
                    ExhibitId = e.ExhibitId,
                    Name = e.Name,
                    Description = e.Description,
                    Materials = e.Materials,
                    IsPermanent = e.IsPermanent,

                    Department = e.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = e.Department.DepartmentId,
                            Name = e.Department.Name
                        }
                        : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<Exhibit> CreateExhibitAsync(Exhibit exhibit)
        {
            _context.Exhibits.Add(exhibit);
            await _context.SaveChangesAsync();
            return exhibit;
        }

        public async Task<Exhibit?> UpdateExhibitAsync(int id, UpdateExhibitRequest request)
        {
            var existing = await _context.Exhibits.FindAsync(id);
            if (existing == null) return null;

            if (request.Image != null && request.Image.Length > 0)
            {
                // удалить старые файлы
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    await _fileService.DeleteFileAsync("exhibits/images", existing.ImagePath);
                    await _fileService.DeleteFileAsync("exhibits/thumbnails", existing.ImagePath);
                }

                // сохранить оригинал
                var fileName = await _fileService.SaveFileAsync(request.Image, "exhibits/images");

                // путь для thumbnail
                var thumbFullPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "exhibits",
                    "thumbnails",
                    fileName
                );

                // создать thumbnail
                await _imageProcessor.SaveAsThumbnailAsync(
                    request.Image,
                    thumbFullPath,
                    width: 300,
                    height: 300
                );

                existing.ImagePath = fileName;
            }

            // обновление остальных полей
            existing.Name = request.Name;
            existing.Description = request.Description;
            existing.Materials = request.Materials;
            existing.IsPermanent = request.IsPermanent;
            existing.DepartmentId = request.DepartmentId;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteExhibitAsync(int id)
        {
            var exhibit = await _context.Exhibits.FindAsync(id);
            if (exhibit == null) return false;

            // Удаление файлов
            if (!string.IsNullOrEmpty(exhibit.ImagePath))
            {
                await _fileService.DeleteFileAsync("exhibits/images", exhibit.ImagePath);
                await _fileService.DeleteFileAsync("exhibits/thumbnails", exhibit.ImagePath);
            }

            _context.Exhibits.Remove(exhibit);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<int> GetExhibitCountAsync() =>
            await _context.Exhibits.CountAsync();

        public async Task<Exhibit?> GetExhibitEntityAsync(int id)
        {
            return await _context.Exhibits
                .FirstOrDefaultAsync(e => e.ExhibitId == id);
        }
    }
}