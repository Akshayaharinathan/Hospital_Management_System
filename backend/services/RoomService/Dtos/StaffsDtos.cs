namespace RoomService.Dtos
{
    public class StaffSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = default!;
        public string Role { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Phone { get; set; } = default!;
    }
}