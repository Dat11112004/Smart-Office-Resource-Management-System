using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Resident
    {
        // ===== Khóa chính =====
        [Key]   // Xác định Id là Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ===== Thông tin cá nhân =====
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(15)]
        [Phone]
        public string Phone { get; set; }

        [Required]
        [MaxLength(20)]
        public string IdentityNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Lecturer, Staff, Guest

        // ===== Ngày nhận & trả phòng =====
        [Required]
        public DateTime CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        // ===== Khóa ngoại: liên kết với phòng =====
        [Required]
        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; }

        // ===== Quan hệ 1-n =====
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public ICollection<Billing> Billings { get; set; } = new List<Billing>();
    }
}
