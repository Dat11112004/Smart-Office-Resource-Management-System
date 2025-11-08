namespace SORMS.API.DTOs
{
    public class CheckInRecordDto
    {
        public int Id { get; set; }
        public int ResidentId { get; set; }
        public string ResidentName { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime? ApprovedTime { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutRequestTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public string Status { get; set; } // PendingCheckIn, CheckedIn, PendingCheckOut, CheckedOut, Rejected
        public string? RejectReason { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public string RequestType { get; set; } // CheckIn, CheckOut
    }

    public class CreateCheckInRequestDto
    {
        public int RoomId { get; set; }
    }

    public class CreateCheckOutRequestDto
    {
        public int CheckInRecordId { get; set; }
    }

    public class ApproveCheckInRequestDto
    {
        public int RequestId { get; set; }
        public bool IsApproved { get; set; } // true = approve, false = reject
        public string? RejectReason { get; set; }
    }
}
