namespace StockService.DTOs
{
    public class PurchaseDto
    {
        public string MedicineName { get; set; } = default!;
        public int Quantity { get; set; }
    }
}