using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class ServiceRequest
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Thông tin dịch vụ =====
        [Required]
        [MaxLength(50)]         // Ví dụ: Maintenance, Cleaning...
        public string ServiceType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        [MaxLength(20)]         // Pending, InProgress, Completed
        public string Status { get; set; }

        // ===== Khóa ngoại: Resident =====
        [Required]
        public int ResidentId { get; set; }

        [ForeignKey(nameof(ResidentId))]
        public Resident Resident { get; set; }

        // ===== Khóa ngoại: Staff =====
        [Required]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; }
    }
}
