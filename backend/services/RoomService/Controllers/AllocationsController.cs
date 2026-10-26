using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.Dtos;
using RoomService.Models;
using RoomService.Services;
using System.Security.Claims;

namespace RoomService.Controllers
{
    [ApiController]
    [Route("api/allocations")]
    public class AllocationsController : ControllerBase
    {
        private readonly RoomDbContext _db;
        private readonly PatientClient _patients;
        private readonly UserClient _users;

        public AllocationsController(RoomDbContext db, PatientClient patients, UserClient users)
        {
            _db = db;
            _patients = patients;
            _users = users;
        }

        [HttpPost]
        [Authorize(Policy = "RequireReceptionist")]
        public async Task<IActionResult> Allocate([FromBody] AllocationRequestDto dto, CancellationToken ct)
        {
            if (!Enum.TryParse<RoomCategory>(dto.Category, true, out var cat))
                return BadRequest("Invalid category. Use AC, NON_AC, GENERAL_WARD");

            // Receptionist path → basic patient data
            var patient = await _patients.GetBasicAsync(dto.PatientId, ct);
            if (patient == null) return NotFound("Patient not found in PatientService");

            // prevent double active allocation
            if (await _db.Allocations.AnyAsync(a => a.PatientId == dto.PatientId && a.EndTime == null, ct))
                return Conflict("Patient already has an active allocation");

            int? roomId = null;
            int? wardBedId = null;

            if (cat == RoomCategory.AC || cat == RoomCategory.NON_AC)
            {
                var availableRoom = await _db.Rooms
                    .Where(r => r.IsActive && r.Category == cat)
                    .Where(r => !_db.Allocations.Any(a => a.RoomId == r.Id && a.EndTime == null))
                    .OrderBy(r => r.RoomNumber)
                    .FirstOrDefaultAsync(ct);

                if (availableRoom == null) return Conflict("No available rooms for selected category");
                roomId = availableRoom.Id;
            }
            else // GENERAL_WARD
            {
                if (string.IsNullOrWhiteSpace(dto.WardName))
                    return BadRequest("WardName is required for GENERAL_WARD");

                var availableBed = await _db.WardBeds
                    .Where(w => w.IsActive && w.WardName == dto.WardName)
                    .Where(w => !_db.Allocations.Any(a => a.WardBedId == w.Id && a.EndTime == null))
                    .OrderBy(w => w.BedNumber)
                    .FirstOrDefaultAsync(ct);

                if (availableBed == null) return Conflict("No available beds in this ward");
                wardBedId = availableBed.Id;
            }

            var receptionistId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "unknown";

            var allocation = new Allocation
            {
                PatientId = dto.PatientId,
                RoomId = roomId,
                WardBedId = wardBedId,
                AllocatedByUserId = receptionistId,
                StartTime = DateTime.UtcNow
            };
            _db.Allocations.Add(allocation);

            try { await _db.SaveChangesAsync(ct); }
            catch (DbUpdateException) { return Conflict("Allocation conflict occurred. Try again."); }

            var response = new AllocationResponseDto
            {
                AllocationId = allocation.Id,
                PatientId = allocation.PatientId,
                Category = cat.ToString(),
                RoomNumber = allocation.RoomId.HasValue ? (await _db.Rooms.FindAsync(new object?[] { allocation.RoomId }, ct))?.RoomNumber : null,
                BedNumber = allocation.WardBedId.HasValue ? (await _db.WardBeds.FindAsync(new object?[] { allocation.WardBedId }, ct))?.BedNumber : null,
                WardName = allocation.WardBedId.HasValue ? (await _db.WardBeds.FindAsync(new object?[] { allocation.WardBedId }, ct))?.WardName : null,
                StartTime = allocation.StartTime
            };

            return Ok(new AllocationWithPatientDto { Allocation = response, Patient = patient, AssignedDoctor = null });
        }

        [HttpGet("patient/{patientId:int}/current")]
        [Authorize] // allow Doctor/Admin/Receptionist; enrich based on role
        public async Task<IActionResult> GetCurrent(int patientId, CancellationToken ct)
        {
            var allocation = await _db.Allocations
                .Include(a => a.Room)
                .Include(a => a.WardBed)
                .Where(a => a.PatientId == patientId && a.EndTime == null)
                .FirstOrDefaultAsync(ct);

            if (allocation == null) return NotFound("No active allocation");

            var response = new AllocationResponseDto
            {
                AllocationId = allocation.Id,
                PatientId = allocation.PatientId,
                Category = allocation.Room?.Category.ToString() ?? RoomCategory.GENERAL_WARD.ToString(),
                RoomNumber = allocation.Room?.RoomNumber,
                BedNumber = allocation.WardBed?.BedNumber,
                WardName = allocation.WardBed?.WardName,
                StartTime = allocation.StartTime
            };

            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var isDoctorOrAdmin = roles.Contains("Doctor") || roles.Contains("Admin");

            PatientSummaryDto? patient;
            StaffSummaryDto? doctor = null;

            if (isDoctorOrAdmin)
            {
                patient = await _patients.GetDetailedAsync(patientId, ct); // Doctor/Admin-only endpoint
                if (patient?.AssignedDoctorId is int did)
                {
                    doctor = await _users.GetStaffByIdAsync(did, ct); // Admin/Doctor-only in UserService
                }
            }
            else
            {
                patient = await _patients.GetBasicAsync(patientId, ct); // Receptionist endpoint
            }

            if (patient == null) return NotFound("Patient not found in PatientService");
            return Ok(new AllocationWithPatientDto { Allocation = response, Patient = patient, AssignedDoctor = doctor });
        }

        [HttpPost("{allocationId}/release")]
        [Authorize(Policy = "RequireReceptionist")]
        public async Task<IActionResult> Release(int allocationId, [FromBody] ReleaseRequestDto dto, CancellationToken ct)
        {
            var allocation = await _db.Allocations.FirstOrDefaultAsync(a => a.Id == allocationId && a.EndTime == null, ct);
            if (allocation == null) return NotFound("Active allocation not found");

            allocation.EndTime = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
            return Ok(new { allocation.Id, allocation.EndTime, message = $"Released: {dto.Reason}" });
        }
    }
}