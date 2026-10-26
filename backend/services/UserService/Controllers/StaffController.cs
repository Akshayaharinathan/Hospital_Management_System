using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly UserDbContext _db;
        public StaffController(UserDbContext db) => _db = db;

        // Admin only: create staff record (note: AuthService should create login separately)
        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] StaffDto dto)
        {
            var s = new Staff
            {
                FullName = dto.FullName,
                Role = dto.Role,
                Email = dto.Email,
                Phone = dto.Phone
            };
            _db.Staffs.Add(s);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = s.Id }, s);
        }

        // Admin only: update staff
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(int id, [FromBody] StaffDto dto)
        {
            var s = await _db.Staffs.FindAsync(id);
            if (s == null) return NotFound();
            s.FullName = dto.FullName;
            s.Role = dto.Role;
            s.Email = dto.Email;
            s.Phone = dto.Phone;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Admin only: delete staff
        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Staffs.FindAsync(id);
            if (s == null) return NotFound();
            _db.Staffs.Remove(s);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // Admin & Doctor: list staff
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Staffs.ToListAsync();
            return Ok(list);
        }

        // Admin & Doctor: get by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetById(int id)
        {
            var s = await _db.Staffs.FindAsync(id);
            if (s == null) return NotFound();
            return Ok(s);
        }
    }
}

