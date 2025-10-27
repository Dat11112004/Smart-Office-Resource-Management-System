using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string RoomNumber { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Type { get; set; } = string.Empty;

        public int Floor { get; set; }

        public decimal MonthlyRent { get; set; }

        public decimal Area { get; set; }

        public bool IsOccupied { get; set; }

        public bool IsAvailable { get; set; }

        public string? Description { get; set; }

        public string? CurrentResident { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Resident> Residents { get; set; } = new List<Resident>();
        public ICollection<CheckInRecord> CheckInRecords { get; set; } = new List<CheckInRecord>();
    }
}
