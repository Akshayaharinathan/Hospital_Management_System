namespace PatientService.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // the id of the staff (doctor) assigned; could be null if not assigned
        public int? AssignedDoctorId { get; set; }

        // For simplicity, store history as long text. In production, create separate records.
        public string MedicalHistory { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}