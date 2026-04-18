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

        public MediaFileController(MediaFileService service)
        {
            _service = service;
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
        public async Task<IActionResult> Create([FromHeader] string token, [FromBody] CreateMediaFileRequest request)
        {
            // Определяем тип медиа по расширению файла
            string? mediaType = GetMediaTypeFromPath(request.FilePath);
            if (mediaType == null)
            {
                return BadRequest(new { status = "error", message = "Unsupported media type" });
            }

            var media = new MediaFile
            {
                FilePath = request.FilePath,
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

            // если путь относительный — собираем полный путь
            var path = Path.IsPathRooted(media.FilePath)
                ? media.FilePath
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", media.FilePath);

            if (!System.IO.File.Exists(path))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetContentType(media.FilePath);

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

                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

                _ => "application/octet-stream"
            };
        }
    }
}