using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Role
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Tên vai trò =====
        [Required]
        [MaxLength(50)]      // Ví dụ: Admin, Manager, Staff, Resident…
        public string Name { get; set; }

        // ===== Mô tả vai trò =====
        [MaxLength(250)]
        public string Description { get; set; }

        // ===== Quan hệ 1-n với User =====
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
