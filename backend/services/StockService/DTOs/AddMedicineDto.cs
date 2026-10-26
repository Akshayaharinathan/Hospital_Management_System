namespace StockService.DTOs
{
    public class AddMedicineDto
    {
        public string Name { get; set; } = default!;
        public int InitialStock { get; set; }
    }

}