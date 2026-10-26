namespace StockService.DTOs
{
    public class PurchaseResultDto
    {
        public string MedicineName { get; set; } = default!;
        public int RequestedQuantity { get; set; }
        public int AvailableBefore { get; set; }
        public int RemainingAfter { get; set; }
        public bool ShouldRestock { get; set; }
    }
}