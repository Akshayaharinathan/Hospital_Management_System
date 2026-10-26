namespace PatientService.DTOs
{
    public class PatientSummaryDto
    {

        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Gender { get; set; }
        public DateTime? Dob  { get; set; }

    }
}
