namespace SORMS.API.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } // 🔹 thêm trường Email
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
    }
}
