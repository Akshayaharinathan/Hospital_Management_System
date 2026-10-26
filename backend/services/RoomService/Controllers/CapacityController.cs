using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomService.Services;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/capacity")]
    [Authorize] // any authenticated user
    public class CapacityController : ControllerBase
    {
        private readonly ICapacityService _capacity;
        public CapacityController(ICapacityService capacity) { _capacity = capacity; }

        [HttpGet("rooms")]
        public async Task<IActionResult> Rooms() => Ok(await _capacity.GetRoomTypeSummaryAsync());

        [HttpGet("wards")]
        public async Task<IActionResult> Wards() => Ok(await _capacity.GetWardSummaryAsync());
    }
}