using Microsoft.EntityFrameworkCore;
using RoomService.Data;
using RoomService.Dtos;
using RoomService.Models;

namespace RoomService.Services
{
    public class CapacityService : ICapacityService
    {
        private readonly RoomDbContext _db;
        public CapacityService(RoomDbContext db) { _db = db; }

        public async Task<List<RoomTypeSummaryDto>> GetRoomTypeSummaryAsync()
        {
            var cats = new[] { RoomCategory.AC, RoomCategory.NON_AC };
            var results = new List<RoomTypeSummaryDto>();
            foreach (var cat in cats)
            {
                var total = await _db.Rooms.Where(r => r.IsActive && r.Category == cat).CountAsync();
                var filled = await _db.Allocations
                    .Where(a => a.EndTime == null && a.RoomId != null && a.Room!.Category == cat)
                    .CountAsync();

                results.Add(new RoomTypeSummaryDto
                {
                    Type = cat.ToString(),
                    Total = total,
                    Filled = filled,
                    Available = Math.Max(0, total - filled)
                });
            }
            return results;
        }

        public async Task<List<WardSummaryDto>> GetWardSummaryAsync()
        {
            var wards = await _db.WardBeds.Where(w => w.IsActive)
                .GroupBy(w => w.WardName)
                .Select(g => new { WardName = g.Key, TotalBeds = g.Count() })
                .ToListAsync();

            var results = new List<WardSummaryDto>();
            foreach (var ward in wards)
            {
                var allocated = await _db.Allocations
                    .Where(a => a.EndTime == null && a.WardBedId != null && a.WardBed!.WardName == ward.WardName)
                    .CountAsync();

                results.Add(new WardSummaryDto
                {
                    WardName = ward.WardName,
                    TotalBeds = ward.TotalBeds,
                    AllocatedBeds = allocated,
                    RemainingBeds = Math.Max(0, ward.TotalBeds - allocated)
                });
            }
            return results;
        }
    }
}