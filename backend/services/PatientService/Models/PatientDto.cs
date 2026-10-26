namespace PatientService.Models
{
    public class PatientDto
    {
        public string Name { get; set; }
        public string Gender { get; set; }
        public DateTime Dob { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int? AssignedDoctorId { get; set; }
        public string MedicalHistory { get; set; }
    }
}