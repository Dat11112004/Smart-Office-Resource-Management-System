using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class CheckInRecord
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Khóa ngoại: Liên kết với cư dân =====
        [Required]
        public int ResidentId { get; set; }

        [ForeignKey(nameof(ResidentId))]
        public Resident Resident { get; set; }

        // ===== Khóa ngoại: Liên kết với phòng =====
        [Required]
        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; }

        // ===== Thời gian check-in / check-out =====
        [Required]
        public DateTime CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        // ===== Trạng thái hiện tại =====
        [Required, MaxLength(20)]
        public string Status { get; set; }   // CheckedIn, CheckedOut

        // ===== Phương thức thực hiện =====
        [Required, MaxLength(20)]
        public string Method { get; set; }   // QR, RFID, Manual
    }
}
