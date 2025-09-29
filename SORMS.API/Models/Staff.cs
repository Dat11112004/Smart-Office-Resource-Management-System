using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Staff
    {
        // ===== Khóa chính =====
        [Key]   // Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Thông tin nhân viên =====
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]           // Validate email format
        [MaxLength(150)]
        public string Email { get; set; }

        [Phone]                  // Validate phone format
        [MaxLength(20)]
        public string Phone { get; set; }

        // ===== Quan hệ =====
        // Một nhân viên có thể được gán nhiều yêu cầu dịch vụ
        public ICollection<ServiceRequest> AssignedRequests { get; set; }
    }
}
