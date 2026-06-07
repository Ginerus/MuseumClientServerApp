using Microsoft.AspNetCore.Mvc;
using MuseumServer.Data;
using Microsoft.EntityFrameworkCore;

namespace MuseumServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly MuseumContext _context;

        public HealthController(MuseumContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // проверка БД (опционально, но полезно)
                var canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                    return StatusCode(503, "DB unavailable");

                return Ok("OK");
            }
            catch
            {
                return StatusCode(503, "Server error");
            }
        }
    }
}