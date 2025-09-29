using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Room
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Số phòng =====
        [Required]
        [MaxLength(20)]        // Ví dụ: A101, B205
        public string RoomNumber { get; set; }

        // ===== Loại phòng =====
        [Required]
        [MaxLength(30)]        // Single, Double, VIP...
        public string Type { get; set; }

        // ===== Trạng thái sử dụng =====
        [Required]
        public bool IsOccupied { get; set; }

        // ===== Quan hệ 1-n với Resident =====
        public ICollection<Resident> Residents { get; set; } = new List<Resident>();
    }
}
