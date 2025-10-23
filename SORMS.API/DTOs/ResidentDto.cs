namespace SORMS.API.DTOs
{
    public class ResidentDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty; // Alias for Phone
        public string IdentityNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Lecturer, Staff, Guest
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public string? Notes { get; set; }
    }
}
