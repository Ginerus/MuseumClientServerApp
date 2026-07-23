using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.Services;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [SessionAuthorize]
    public class MuseumInfoController : ControllerBase
    {
        private readonly MuseumInfoService _service;

        public MuseumInfoController(MuseumInfoService service)
        {
            _service = service;
        }

        // GET: api/MuseumInfo
        [HttpGet]
        public async Task<IActionResult> Get([FromHeader] string token)
        {
            var description = await _service.GetDescriptionAsync();
            return Ok(new { status = "ok", data = new { description } });
        }

        // PUT: api/MuseumInfo
        [HttpPut]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> Update(
            [FromHeader] string token,
            [FromBody] UpdateMuseumInfoRequest request)
        {
            var updated = await _service.UpdateDescriptionAsync(request.Description ?? string.Empty);
            return Ok(new { status = "ok", data = new { description = updated } });
        }
    }

    public class UpdateMuseumInfoRequest
    {
        public string? Description { get; set; }
    }
}