using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class ServiceRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string ServiceType { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; }

        // FK: Resident
        [Required]
        public int ResidentId { get; set; }

        [ForeignKey(nameof(ResidentId))]
        public Resident Resident { get; set; }

        // FK: Staff
        [Required]
        public int StaffId { get; set; }

        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; }
    }
}
