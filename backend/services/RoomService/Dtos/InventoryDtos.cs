namespace RoomService.Dtos
{
    public class RoomCreateDto
    {
        public string RoomNumber { get; set; } = default!;
        public string Category { get; set; } = default!; // AC/NON_AC
    }

    public class WardBedCreateDto
    {
        public string WardName { get; set; } = default!;
        public string BedNumber { get; set; } = default!;
    }

    public class RoomTypeSummaryDto
    {
        public string Type { get; set; } = default!; // AC/NON_AC
        public int Total { get; set; }
        public int Filled { get; set; }
        public int Available { get; set; }
    }

    public class WardSummaryDto
    {
        public string WardName { get; set; } = default!;
        public int TotalBeds { get; set; }
        public int AllocatedBeds { get; set; }
        public int RemainingBeds { get; set; }
    }
}
