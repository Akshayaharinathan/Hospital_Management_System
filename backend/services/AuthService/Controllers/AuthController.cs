using Microsoft.AspNetCore.Mvc;
using AuthService.Data;
using AuthService.Models;
using AuthService.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(AuthDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
                return BadRequest("Username taken");

            CreatePasswordHash(dto.Password, out byte[] hash, out byte[] salt);
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = hash,
                PasswordSalt = salt,
                Roles = dto.Roles // e.g. "Admin" or "Doctor,Receptionist"
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(new { user.Id, user.Username, user.Roles });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Unauthorized("Invalid credentials");

            if (!VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                return Unauthorized("Invalid credentials");

            var token = _tokenService.CreateToken(user);
            return Ok(new { token });
        }

        // helpers
        private static void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private static bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computed.SequenceEqual(storedHash);
        }
    }

    // DTOs
    public class RegisterDto { public string Username { get; set; } public string Password { get; set; } public string Roles { get; set; } }
    public class LoginDto { public string Username { get; set; } public string Password { get; set; } }
}
