using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Models;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class MediaFileController : ControllerBase
    {
        private readonly MediaFileService _service;
        private readonly IWebHostEnvironment _env;
        private readonly IFileService _fileService;
        private readonly ImageProcessor _imageProcessor;

        // Конструктор
        public MediaFileController(
            MediaFileService service,
            IWebHostEnvironment env,
            IFileService fileService,
            ImageProcessor imageProcessor)
        {
            _service = service;
            _env = env;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
        }

        // GET: api/MediaFiles
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var mediaFiles = await _service.GetAllAsync();
            return Ok(new { status = "ok", data = mediaFiles });
        }

        // GET: api/MediaFiles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader] string token, int id)
        {
            var media = await _service.GetAsync(id);
            if (media == null)
                return NotFound(new { status = "error", message = "Media file not found" });

            return Ok(new { status = "ok", data = media });
        }

        // POST: api/MediaFiles
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create(
            [FromHeader] string token,
            [FromForm] CreateMediaFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { status = "error", message = "File is required" });

            var mediaType = GetMediaTypeFromPath(request.File.FileName);
            if (mediaType == null)
                return BadRequest(new { status = "error", message = "Unsupported media type" });

            var folder = mediaType switch
            {
                "image" => "media/images/original",
                "video" => "media/videos",
                _ => "media/other"
            };

            // Используем FileService
            var fileName = await _fileService.SaveFileAsync(request.File, folder);

            if (mediaType == "image")
            {
                var thumbnailFolder = "media/images/thumbnails";

                var thumbnailFullPath = Path.Combine(
                    _env.WebRootPath,
                    thumbnailFolder,
                    fileName
                );

                await _imageProcessor.SaveAsThumbnailAsync(
                    request.File,
                    thumbnailFullPath,
                    300,
                    300
                );
            }

            var media = new MediaFile
            {
                Title = request.Title,
                FilePath = $"{folder}/{fileName}",
                MediaType = mediaType,
                Description = request.Description,
                DepartmentId = request.DepartmentId
            };

            var created = await _service.CreateAsync(media);

            return Ok(new { status = "ok", data = created });
        }

        // DELETE: api/MediaFiles/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Media file not found" });

            return Ok(new { status = "ok" });
        }

        // GET: api/MediaFiles/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var mediaFiles = await _service.GetAllAsync();
            return Ok(new { status = "ok", count = mediaFiles.Count });
        }

        // GET: api/MediaFile/stream/{id}
        [HttpGet("stream/{id}")]
        public async Task<IActionResult> Stream([FromHeader] string token, int id)
        {
            var media = await _service.GetEntityAsync(id);

            if (media == null || string.IsNullOrEmpty(media.FilePath))
                return NotFound(new { status = "error", message = "Media file not found" });

            var path = Path.IsPathRooted(media.FilePath)
                ? media.FilePath
                : Path.Combine(_env.WebRootPath, media.FilePath);

            if (!System.IO.File.Exists(path))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetContentType(media.FilePath);

            // Отключение кэша для статики
            Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return PhysicalFile(path, contentType, enableRangeProcessing: true);
        }

        // Вспомогательный метод для определения типа медиа
        private string? GetMediaTypeFromPath(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant().TrimStart('.');
            if (ext == null) return null;

            return ext switch
            {
                "jpg" => "image",
                "jpeg" => "image",
                "png" => "image",
                "gif" => "image",
                "mp4" => "video",
                "mov" => "video",
                "avi" => "video",
                _ => null
            };
        }

        // Определения MIME-типа (Content-Type) файла по его расширению.
        private string GetContentType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",

                ".mp4" => "video/mp4",
                ".mov" => "video/quicktime",
                ".avi" => "video/x-msvideo",

                _ => "application/octet-stream"
            };
        }
    }
}