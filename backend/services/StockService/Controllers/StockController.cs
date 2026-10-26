using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockService.Data;
using StockService.DTOs;
using StockService.Models;

namespace StockService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize(Policy = "Pharmacist,Admin")]
    public class StockController : ControllerBase
    {
        private readonly StockDbContext _db;

        // Internal threshold (DO NOT accept as input)
        private const int ReorderThreshold = 5;

        public StockController(StockDbContext db) => _db = db;

        // ---------- Create / Add Medicine ----------
        [HttpPost("medicines")]
        [Authorize(Roles = "Admin,Pharmacist")]

        public async Task<IActionResult> AddMedicine([FromBody] AddMedicineDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Medicine name is required.");

            var exists = await _db.Medicines.AnyAsync(m => m.Name == dto.Name);
            if (exists) return Conflict($"Medicine '{dto.Name}' already exists.");

            var med = new Medicine
            {
                Name = dto.Name.Trim(),
                TotalStock = Math.Max(0, dto.InitialStock),
            };

            _db.Medicines.Add(med);

            // Log addition
            _db.StockTransactions.Add(new StockTransaction
            {
                Medicine = med,
                QuantityChange = med.TotalStock,
                Type = TransactionType.Addition,
                PerformedByUserId = User?.Identity?.Name
            });

            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAvailabilityByName),
                new { name = med.Name },
                new
                {
                    med.Id,
                    med.Name,
                    med.TotalStock,
                    ShouldRestock = med.TotalStock <= ReorderThreshold
                });
        }

        // ---------- Increase Stock for Existing Medicine ----------
        [HttpPost("medicines/{id:int}/add")]
        [Authorize(Roles = "Admin,Pharmacist")]

        public async Task<IActionResult> AddStock([FromRoute] int id, [FromBody] AddStockDto dto)
        {
            if (dto.Quantity <= 0) return BadRequest("Quantity must be positive.");

            var med = await _db.Medicines.FirstOrDefaultAsync(m => m.Id == id);
            if (med == null) return NotFound($"Medicine with id {id} not found.");

            med.TotalStock += dto.Quantity;
            med.UpdatedAt = DateTime.UtcNow;

            _db.StockTransactions.Add(new StockTransaction
            {
                MedicineId = med.Id,
                QuantityChange = dto.Quantity,
                Type = TransactionType.Addition,
                PerformedByUserId = User?.Identity?.Name
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                med.Id,
                med.Name,
                med.TotalStock,
                ShouldRestock = med.TotalStock <= ReorderThreshold
            });
        }

        // ---------- Purchase ----------
        // Pharmacist enters name + quantity given to the patient
        [HttpPost("purchase")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<ActionResult<PurchaseResultDto>> Purchase([FromBody] PurchaseDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MedicineName))
                return BadRequest("MedicineName is required.");
            if (dto.Quantity <= 0)
                return BadRequest("Quantity must be positive.");

            var med = await _db.Medicines
                .FirstOrDefaultAsync(m => m.Name == dto.MedicineName.Trim());

            if (med == null)
                return NotFound($"Medicine '{dto.MedicineName}' not found.");

            var availableBefore = med.TotalStock;

            if (availableBefore < dto.Quantity)
                return BadRequest(new
                {
                    Message = $"Insufficient stock. Available: {availableBefore}",
                    MedicineName = med.Name,
                    Requested = dto.Quantity
                });

            med.TotalStock -= dto.Quantity;
            med.UpdatedAt = DateTime.UtcNow;

            _db.StockTransactions.Add(new StockTransaction
            {
                MedicineId = med.Id,
                QuantityChange = -dto.Quantity,
                Type = TransactionType.Purchase,
                PerformedByUserId = User?.Identity?.Name
            });

            // Persist
            await _db.SaveChangesAsync();

            var result = new PurchaseResultDto
            {
                MedicineName = med.Name,
                RequestedQuantity = dto.Quantity,
                AvailableBefore = availableBefore,
                RemainingAfter = med.TotalStock,
                ShouldRestock = med.TotalStock <= ReorderThreshold
            };

            return Ok(result);
        }

        // ---------- Availability ----------
        [HttpGet("medicines/{name}/available")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> GetAvailabilityByName([FromRoute] string name)
        {
            var med = await _db.Medicines.FirstOrDefaultAsync(m => m.Name == name);
            if (med == null) return NotFound($"Medicine '{name}' not found.");
            return Ok(new
            {
                med.Id,
                med.Name,
                Available = med.TotalStock,
                ShouldRestock = med.TotalStock <= ReorderThreshold
            });
        }

        // ---------- List all medicines & stocks ----------
        [HttpGet("medicines")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> GetAllMedicines()
        {
            var meds = await _db.Medicines
                .OrderBy(m => m.Name)
                .Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.TotalStock,
                    ShouldRestock = m.TotalStock <= ReorderThreshold
                })
                .ToListAsync();

            return Ok(meds);
        }

        // ---------- Restock list (≤ 5) ----------
        [HttpGet("restock")]
        [Authorize(Roles = "Admin,Pharmacist")]
        public async Task<IActionResult> GetRestockList()
        {
            var toRestock = await _db.Medicines
                .Where(m => m.TotalStock <= ReorderThreshold)
                .OrderBy(m => m.TotalStock)
                .Select(m => new { m.Name, m.TotalStock })
                .ToListAsync();

            // If stocks are below/at 5, show names to be repurchased.
            return Ok(new
            {
                Threshold = ReorderThreshold,
                Count = toRestock.Count,
                Medicines = toRestock.Select(m => m.Name).ToList()
            });
        }
    }
}