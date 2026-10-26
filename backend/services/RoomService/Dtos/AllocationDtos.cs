namespace RoomService.Dtos
{
    public class AllocationRequestDto
    {
        public int PatientId { get; set; }
        public string Category { get; set; } = default!;   // "AC" | "NON_AC" | "GENERAL_WARD"
        public string? WardName { get; set; }              // required for GENERAL_WARD
    }

    public class AllocationResponseDto
    {
        public int AllocationId { get; set; }
        public int PatientId { get; set; }
        public string Category { get; set; } = default!;
        public string? RoomNumber { get; set; }
        public string? BedNumber { get; set; }
        public string? WardName { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class ReleaseRequestDto
    {
        public string Reason { get; set; } = default!;
    }

    // Patient summary used by receptionist/doctor views
    public class PatientSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Gender { get; set; } = default!;
        public DateTime Dob { get; set; }
        public string Phone { get; set; } = default!;
        public string Address { get; set; } = default!;
        public int? AssignedDoctorId { get; set; }
        // MedicalHistory omitted here by design for receptionist path
    }

    public class AllocationWithPatientDto
    {
        public AllocationResponseDto Allocation { get; set; } = default!;
        public PatientSummaryDto Patient { get; set; } = default!;
        public StaffSummaryDto? AssignedDoctor { get; set; } // optional enrichment
    }
}
