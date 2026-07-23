using Microsoft.AspNetCore.Mvc;
using MuseumServer.Attributes;
using MuseumServer.Services;
using MuseumServer.DTO;

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

        [HttpPut("password")]
        [SessionAuthorize(adminOnly: true)]
        public async Task<IActionResult> ChangePassword([FromHeader] string token, [FromBody] ChangePasswordRequest request)
        {
            var result = await _service.ChangeAdminPasswordAsync(
                request.OldPassword,
                request.NewPassword);

            if (!result)
                return BadRequest(new
                {
                    status = "error",
                    message = "OLD_PASSWORD_INVALID"
                });

            return Ok(new
            {
                status = "ok"
            });
        }
    }

    public class UpdateMuseumInfoRequest
    {
        public string? Description { get; set; }
    }
}