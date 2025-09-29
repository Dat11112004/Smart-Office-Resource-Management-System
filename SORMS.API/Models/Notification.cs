using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Notification
    {
        // ===== Khóa chính =====
        [Key]   // Chỉ rõ đây là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Nội dung thông báo =====
        [Required]
        [MaxLength(500)] // Giới hạn độ dài để tránh text quá lớn
        public string Message { get; set; }

        // ===== Thời điểm tạo =====
        [Required]
        public DateTime CreatedAt { get; set; }

        // ===== Trạng thái đã đọc =====
        [Required]
        public bool IsRead { get; set; }

        // ===== Khóa ngoại: Liên kết với Resident =====
        [Required]
        public int ResidentId { get; set; }

        [ForeignKey(nameof(ResidentId))]
        public Resident Resident { get; set; }
    }
}
