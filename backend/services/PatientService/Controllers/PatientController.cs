
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PatientService.Data;
using PatientService.Models;
using System.Security.Claims;
using PatientService.DTOs;
 
namespace PatientService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly PatientDbContext _db;
        public PatientController(PatientDbContext db) => _db = db;

        // Receptionist: register new patient
        [HttpPost]
        [Authorize(Policy = "RequireReceptionist")]
        public async Task<IActionResult> Register([FromBody] PatientDto dto)
        {
            var p = new Patient
            {
                Name = dto.Name,
                Gender = dto.Gender,
                Dob = dto.Dob,
                Phone = dto.Phone,
                Address = dto.Address,
                AssignedDoctorId = dto.AssignedDoctorId,
                MedicalHistory = dto.MedicalHistory
            };
            _db.Patients.Add(p);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = p.Id }, p);
        }

        // Receptionist: update patient
        [HttpPut("{id}")]
        [Authorize(Policy = "RequireReceptionist")]
        public async Task<IActionResult> Update(int id, [FromBody] PatientDto dto)
        {
            var p = await _db.Patients.FindAsync(id);
            if (p == null) return NotFound();
            p.Name = dto.Name;
            p.Gender = dto.Gender;
            p.Dob = dto.Dob;
            p.Phone = dto.Phone;
            p.Address = dto.Address;
            p.AssignedDoctorId = dto.AssignedDoctorId;
            p.MedicalHistory = dto.MedicalHistory;
            await _db.SaveChangesAsync();
            return NoContent();
        }



        //Doctor: get patient by id only if assigned to them OR Admin can view any
       [HttpGet("{id}")]
        [Authorize(Roles = "Doctor,Admin,Receptionist")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _db.Patients.FindAsync(id);
            if (p == null) return NotFound();

            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isAdmin = userRoles.Contains("Admin");
            if (isAdmin) return Ok(p);
            if (userRoles.Contains("Receptionist"))
                return Ok(p);

            // ensure doctor can only access if assignedDoctorId matches their staff id
            // Here we assume the JWT contains a claim "uid" for user id; AuthService seeded staff/user ids should align.
            var uidClaim = User.FindFirst("uid")?.Value;
            if (int.TryParse(uidClaim, out int uid))
            {
                if (p.AssignedDoctorId == uid) return Ok(p);
            }

            return Forbid();
        }

        //[HttpGet("{id}")]
        //[Authorize(Roles = "Doctor,Admin,Receptionist")]
        //public async Task<IActionResult> GetById(int id)
        //{
        //    var p = await _db.Patients.FindAsync(id);
        //    if (p == null) return NotFound();

        //    var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        //    if (roles.Contains("Admin")) return Ok(p);

        //    // Receptionist gets minimal info
        //    if (roles.Contains("Receptionist"))
        //    {
        //        var basic = new PatientSummaryDto
        //        {
        //            Id = p.Id,
        //            Name = p.Name,
        //            Gender = p.Gender,
        //            Dob = p.Dob
        //        };
        //        return Ok(basic);
        //    }

        //    // Doctor must be assigned
        //    var uidClaim = User.FindFirst("uid")?.Value;
        //    if (int.TryParse(uidClaim, out int uid) && p.AssignedDoctorId == uid)
        //        return Ok(p);

        //    return Forbid();
        //}

            // Admin: list all patients
            [HttpGet]
        [Authorize(Roles = "Doctor,Admin,Receptionist")]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Patients.ToListAsync();
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isAdmin = userRoles.Contains("Admin");
            var isReceptionist= userRoles.Contains("Receptionist");
            if (isAdmin||isReceptionist)
            return Ok(list);
            else
            {
                var uidClaim = Convert.ToInt32(User.FindFirst("uid")?.Value);
                var docpatient = list.FirstOrDefault(p => p.AssignedDoctorId == uidClaim);
                if(docpatient==null)
                return NoContent();
                return Ok(docpatient);
            }
        }
    }
}
