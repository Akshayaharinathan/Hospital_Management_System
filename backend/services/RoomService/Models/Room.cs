using System.ComponentModel.DataAnnotations;

namespace RoomService.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        public RoomCategory Category { get; set; } // AC or NON_AC

        [Required, MaxLength(20)]
        public string RoomNumber { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}