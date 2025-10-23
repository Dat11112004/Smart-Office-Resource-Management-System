namespace SORMS.API.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty; // Alias for Type
        public int Floor { get; set; }
        public decimal MonthlyRent { get; set; }
        public decimal Area { get; set; }
        public bool IsOccupied { get; set; }
        public bool IsAvailable { get; set; } // Opposite of IsOccupied
        public string? CurrentResident { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
