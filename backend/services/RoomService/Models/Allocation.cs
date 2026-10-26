using System.ComponentModel.DataAnnotations;

namespace RoomService.Models
{
    public class Allocation
    {
        public int Id { get; set; }

        [Required]
        public int PatientId { get; set; } // matches PatientService int Id

        public int? RoomId { get; set; }
        public Room? Room { get; set; }

        public int? WardBedId { get; set; }
        public WardBed? WardBed { get; set; }

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }

        [Required]
        public string AllocatedByUserId { get; set; } = default!; // receptionist user id/username from JWT
    }
}
