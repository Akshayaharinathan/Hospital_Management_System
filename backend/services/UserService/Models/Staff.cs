namespace UserService.Models
{
    public class Staff
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // e.g. "Doctor" or "Receptionist" or "Pharmacist" or "Admin"
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
    }
}