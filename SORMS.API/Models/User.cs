using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class User
    {
        // ===== Khóa chính =====
        [Key]   // Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Thông tin tài khoản =====
        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }  // Lưu chuỗi hash thay vì mật khẩu gốc

        // ===== Quan hệ =====
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public Role Role { get; set; }  // Mỗi User thuộc một Role

        // ===== Trạng thái =====
        public bool IsActive { get; set; } = true; // Mặc định tài khoản hoạt động
    }
}
