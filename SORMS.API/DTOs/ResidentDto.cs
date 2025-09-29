namespace SORMS.API.DTOs
{
    public class ResidentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string IdentityNumber { get; set; }
        public string Role { get; set; } // Lecturer, Staff, Guest
        public int RoomId { get; set; }
    }

}
