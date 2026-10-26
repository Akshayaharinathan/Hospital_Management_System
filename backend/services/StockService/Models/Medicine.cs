using System.ComponentModel.DataAnnotations;

namespace StockService.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = default!;

        [Range(0, int.MaxValue)]
        public int TotalStock { get; set; }

        // Concurrency control
        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}