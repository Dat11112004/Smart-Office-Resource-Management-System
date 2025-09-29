namespace SORMS.API.DTOs
{
    public class CheckInRecordDto
    {
        public int Id { get; set; }

        public int ResidentId { get; set; }
        public string ResidentName { get; set; }

        public int RoomId { get; set; }
        public string RoomNumber { get; set; }

        public DateTime CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }

        public string Status { get; set; } // CheckedIn, CheckedOut
        public string Method { get; set; } // QR, RFID, Manual
    }

}
