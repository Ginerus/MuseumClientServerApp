using Microsoft.AspNetCore.Mvc;
using MuseumServer.DTOs;
using MuseumServer.Models;
using MuseumServer.Services;
using MuseumServer.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]

    public class ExhibitController : ControllerBase
    {
        private readonly ExhibitService _service;

        public ExhibitController(ExhibitService service)
        {
            _service = service;
        }

        // GET: api/exhibit
        [HttpGet]
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var exhibits = await _service.GetAllExhibitsAsync();
            return Ok(new { status = "ok", data = exhibits });
        }

        // GET: api/exhibit/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromHeader] string token, int id)
        {
            var exhibit = await _service.GetExhibitAsync(id);
            if (exhibit == null)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        // GET: api/exhibit/count
        [HttpGet("count")]
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var count = await _service.GetExhibitCountAsync();
            return Ok(new { status = "ok", count });
        }

        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create( [FromHeader] string token, [FromForm] CreateExhibitRequest request)
        {
            if (request.Image == null || request.Image.Length == 0)
                return BadRequest("Image is required");

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesPath = Path.Combine(basePath, "exhibits", "images");
            var thumbsPath = Path.Combine(basePath, "exhibits", "thumbnails");

            Directory.CreateDirectory(imagesPath);
            Directory.CreateDirectory(thumbsPath);

            var ext = Path.GetExtension(request.Image.FileName).ToLowerInvariant();
            var fileName = GenerateFileName(ext);

            var imageFullPath = Path.Combine(imagesPath, fileName);
            var thumbFullPath = Path.Combine(thumbsPath, fileName);

            // 1. Сохраняем оригинал
            using (var stream = new FileStream(imageFullPath, FileMode.Create))
            {
                await request.Image.CopyToAsync(stream);
            }

            // 2. Делаем миниатюру
            using (var image = await SixLabors.ImageSharp.Image.LoadAsync(request.Image.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(200, 200));
                await image.SaveAsync(thumbFullPath);
            }

            // 3. Сохраняем в БД
            var exhibit = new Exhibit
            {
                Name = request.Name,
                Description = request.Description,
                Materials = request.Materials,
                IsPermanent = request.IsPermanent,
                DepartmentId = request.DepartmentId,
                ImagePath = fileName // одно имя для двух папок
            };

            var created = await _service.CreateExhibitAsync(exhibit);

            return Ok(new { status = "ok", data = created });
        }

        // PUT: api/exhibit/{id}
        [HttpPut("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update([FromHeader] string token, int id, [FromBody] UpdateExhibitRequest request)
        {
            var exhibit = new Exhibit
            {
                ExhibitId = id, // id берём из URL
                Name = request.Name,
                Description = request.Description,
                Materials = request.Materials,
                IsPermanent = request.IsPermanent,
                ImagePath= request.ImagePath,
                DepartmentId = request.DepartmentId
            };

            var updated = await _service.UpdateExhibitAsync(exhibit);
            if (!updated)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok", data = exhibit });
        }

        // DELETE: api/exhibit/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var deleted = await _service.DeleteExhibitAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Exhibit not found" });

            return Ok(new { status = "ok" });
        }

        // GET: api/exhibit/image/{id}
        [HttpGet("image/{id}")]
        public async Task<IActionResult> GetImage([FromHeader] string token, int id)
        {
            var exhibit = await _service.GetExhibitAsync(id);

            if (exhibit == null || string.IsNullOrEmpty(exhibit.ImagePath))
                return NotFound(new { status = "error", message = "Image not found" });

            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var imagesRoot = Path.Combine(basePath, "exhibits", "images");

            var path = Path.IsPathRooted(exhibit.ImagePath)
                ? exhibit.ImagePath
                : Path.Combine(imagesRoot, exhibit.ImagePath);

            var fullPath = Path.GetFullPath(path);

            Console.WriteLine(fullPath);

            // защита от выхода за папку images
            if (!fullPath.StartsWith(imagesRoot))
                return BadRequest(new { status = "error", message = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { status = "error", message = "File not found on disk" });

            var contentType = GetImageContentType(fullPath);

            return PhysicalFile(fullPath, contentType, enableRangeProcessing: true);
        }

        // Определение типа данных
        private string GetImageContentType(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }

        // Генерация уникального имени файла
        private string GenerateFileName(string extension)
        {
            var date = DateTime.UtcNow.ToString("ddMMyyyy");
            var random = new Random().Next(10000000, 99999999);

            return $"exh{date}{random}{extension}";
        }
    }
}