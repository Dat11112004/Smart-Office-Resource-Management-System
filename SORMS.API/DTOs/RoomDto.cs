namespace SORMS.API.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public bool IsOccupied { get; set; }
    }

}
