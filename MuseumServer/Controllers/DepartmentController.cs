using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.DTOs;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentService _service;

        public DepartmentController(DepartmentService service)
        {
            _service = service;
        }

        // GET: api/departments/count
        [HttpGet("count")]
        [SessionAuthorize] // Guest + Admin
        public async Task<IActionResult> GetCount([FromHeader] string token)
        {
            var count = await _service.GetCountAsync();
            return Ok(new { status = "ok", count });
        }

        // GET: api/department
        [HttpGet]
        [SessionAuthorize] // Guest + Admin
        public async Task<IActionResult> GetAll([FromHeader] string token)
        {
            var depts = await _service.GetAllAsync();
            return Ok(new { status = "ok", data = depts });
        }

        // GET: api/department/{id}
        [HttpGet("{id}")]
        [SessionAuthorize] // Guest + Admin
        public async Task<IActionResult> GetContent([FromHeader] string token, int id, [FromQuery] string? types)
        {
            var content = await _service.GetContentAsync(id, types);
            if (content == null)
                return NotFound(new { status = "error", message = "Department not found" });

            return Ok(new { status = "ok", data = content });
        }

        // POST: api/department
        [HttpPost]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Create(
            [FromHeader] string token,
            [FromForm] CreateDepartmentRequest request,
            [FromServices] ImageService imageService)
        {
            string? imageName = null;

            if (request.Image != null)
            {
                var result = await imageService.SaveImageWithThumbnailAsync(
                    request.Image,
                    "departments"
                );

                imageName = result.fileName;
            }

            var dept = await _service.CreateAsync(request, imageName);

            return Ok(new
            {
                status = "ok",
                data = dept
            });
        }

        // DELETE: api/department/{id}
        [HttpDelete("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Delete([FromHeader] string token, int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { status = "error", message = "Department not found" });

            return Ok(new { status = "ok" });
        }

        // PUT: api/department/{id}
        [HttpPut("{id}")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update([FromHeader] string token, int id, [FromBody] UpdateDepartmentRequest request)
        {
            var updated = await _service.UpdateAsync(id, request);
            if (!updated)
                return NotFound(new { status = "error", message = "Department not found" });

            return Ok(new { status = "ok" });
        }
    }
}