using System.ComponentModel.DataAnnotations;

namespace RoomService.Models
{
    public class WardBed
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string WardName { get; set; } = default!;

        [Required, MaxLength(20)]
        public string BedNumber { get; set; } = default!;

        public bool IsActive { get; set; } = true;
    }
}
