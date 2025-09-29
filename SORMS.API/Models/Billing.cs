using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Billing
    {
        // ---- Khóa chính ----
        [Key]   // Chỉ rõ đây là Primary Key, giữ nguyên tên Id
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime BillingDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // kiểu tiền chính xác hơn
        public decimal Amount { get; set; }

        public bool IsPaid { get; set; }

        // ---- Khóa ngoại ----
        [Required]
        public int ResidentId { get; set; }

        [ForeignKey(nameof(ResidentId))]
        public Resident Resident { get; set; }
    }
}
