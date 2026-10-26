namespace AuthService.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        // one user may have multiple roles; store as comma-separated for simplicity
        public string Roles { get; set; } // e.g. "Admin,Doctor,Receptionist"
    }
}
