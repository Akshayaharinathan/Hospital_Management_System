using Microsoft.AspNetCore.Mvc;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet] public IActionResult Get() => Ok(new { status = "ok" });
    }
}
