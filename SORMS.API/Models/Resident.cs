using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Resident
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(15), Phone]
        public string Phone { get; set; }

        [Required, MaxLength(20)]
        public string IdentityNumber { get; set; }

        [Required, MaxLength(20)]
        public string Role { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        public DateTime? CheckOutDate { get; set; }

        // FK: Room
        [Required]
        public int RoomId { get; set; }

        [ForeignKey(nameof(RoomId))]
        public Room Room { get; set; }

        // Navigation collections
        public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
        public ICollection<Billing> Billings { get; set; } = new List<Billing>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<CheckInRecord> CheckInRecords { get; set; } = new List<CheckInRecord>();
    }
}
