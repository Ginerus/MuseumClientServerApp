using Microsoft.EntityFrameworkCore;
using MuseumServer.Data;
using MuseumServer.DTOs;
using MuseumServer.Models;
using static MuseumServer.DTOs.DocumentFullResponse;

namespace MuseumServer.Services
{
    public class DocumentService
    {
        private readonly MuseumContext _context;
        private readonly IFileService _fileService;

        public DocumentService(MuseumContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // Получить все документы
        public async Task<List<DocumentWithDepartmentResponse>> GetAllDocumentsAsync()
        {
            return await _context.Documents
                .Select(d => new DocumentWithDepartmentResponse
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    FileType = d.FileType,

                    Department = d.Department != null
                        ? new DepartmentInfo
                        {
                            DepartmentId = d.Department.DepartmentId,
                            Name = d.Department.Name
                        }
                        : null
                })
                .ToListAsync();
        }

        // Получить один документ по id
        public async Task<DocumentFullResponse?> GetDocumentAsync(int id)
        {
            return await _context.Documents
                .Where(d => d.DocumentId == id)
                .Select(d => new DocumentFullResponse
                {
                    DocumentId = d.DocumentId,
                    Title = d.Title,
                    FilePath = d.FilePath,
                    FileType = d.FileType,
                    ExhibitId = d.ExhibitId,
                    Department = d.Department != null ? new DepartmentInfo
                    {
                        DepartmentId = d.Department.DepartmentId,
                        Name = d.Department.Name,
                    }
                    : null // если отдела нет, оставляем null
                })
                .FirstOrDefaultAsync();
        }

        // Получить количество документов
        public async Task<int> GetDocumentCountAsync() =>
            await _context.Documents.CountAsync();

        // Загрузить документ
        public async Task<DocumentFullResponse> CreateDocumentAsync(CreateDocumentRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ArgumentException("File is required");

            var ext = Path.GetExtension(request.File.FileName)?.ToLowerInvariant();

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".md" };

            if (ext == null || !allowedExtensions.Contains(ext))
                throw new ArgumentException("Unsupported file type");

            var fileName = await _fileService.SaveFileAsync(request.File, "documents");

            var document = new Document
            {
                Title = request.Title,
                FilePath = Path.Combine("documents", fileName).Replace("\\", "/"),
                FileType = ext.TrimStart('.'),
                ExhibitId = request.ExhibitId,
                DepartmentId = request.DepartmentId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return new DocumentFullResponse
            {
                DocumentId = document.DocumentId,
                Title = document.Title,
                FilePath = document.FilePath,
                FileType = document.FileType,
                ExhibitId = document.ExhibitId,
                Department = null
            };
        }

        // Удалить документ
        public async Task<bool> DeleteDocumentWithFileAsync(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null) return false;

            if (!string.IsNullOrEmpty(document.FilePath))
            {
                var folder = Path.GetDirectoryName(document.FilePath);
                var fileName = Path.GetFileName(document.FilePath);

                if (folder != null && fileName != null)
                    await _fileService.DeleteFileAsync(folder, fileName);
            }

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return true;
        }

        // Получить сущность документа по id (для стриминга)
        public async Task<Document?> GetDocumentEntityAsync(int id)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.DocumentId == id);
        }
    }
}