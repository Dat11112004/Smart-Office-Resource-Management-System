using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Report
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Tiêu đề báo cáo =====
        [Required]
        [MaxLength(200)]      // Giới hạn độ dài hợp lý cho tiêu đề
        public string Title { get; set; }

        // ===== Ngày tạo/Ngày xuất báo cáo =====
        [Required]
        public DateTime GeneratedDate { get; set; }

        // ===== Nội dung báo cáo =====
        [Required]
        public string Content { get; set; }  // Có thể lưu JSON hoặc HTML

        // ===== Người tạo báo cáo =====
        [Required]
        [MaxLength(100)]
        public string CreatedBy { get; set; }
    }
}
