using System.ComponentModel.DataAnnotations;

namespace StockService.Models
{
    public enum TransactionType { Addition = 1, Purchase = 2 }

    public class StockTransaction
    {
        public int Id { get; set; }

        public int MedicineId { get; set; }
        public Medicine Medicine { get; set; } = default!;

        // Negative quantity for purchase, positive for addition
        public int QuantityChange { get; set; }

        public TransactionType Type { get; set; }

        public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

        // Optional: who performed the action
        [MaxLength(100)]
        public string? PerformedByUserId { get; set; }
    }
}