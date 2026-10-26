using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.Dtos;
using RoomService.Models;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly RoomDbContext _db;
        public InventoryController(RoomDbContext db) { _db = db; }

        [HttpPost("rooms")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> AddRoom([FromBody] RoomCreateDto dto)
        {
            if (!Enum.TryParse<RoomCategory>(dto.Category, true, out var cat) || cat == RoomCategory.GENERAL_WARD)
                return BadRequest("Category must be AC or NON_AC");

            if (await _db.Rooms.AnyAsync(r => r.RoomNumber == dto.RoomNumber))
                return Conflict("Room number already exists");

            _db.Rooms.Add(new Room { RoomNumber = dto.RoomNumber, Category = cat, IsActive = true });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("ward-beds")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> AddWardBed([FromBody] WardBedCreateDto dto)
        {
            if (await _db.WardBeds.AnyAsync(w => w.WardName == dto.WardName && w.BedNumber == dto.BedNumber))
                return Conflict("Ward bed already exists");

            _db.WardBeds.Add(new WardBed { WardName = dto.WardName, BedNumber = dto.BedNumber, IsActive = true });
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
