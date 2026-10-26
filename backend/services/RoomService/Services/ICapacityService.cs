using RoomService.Dtos;

namespace RoomService.Services
{
    public interface ICapacityService
    {
        Task<List<RoomTypeSummaryDto>> GetRoomTypeSummaryAsync();
        Task<List<WardSummaryDto>> GetWardSummaryAsync();
    }
}