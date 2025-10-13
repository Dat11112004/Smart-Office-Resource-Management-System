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
        public string RoomNumber { get; set; }

        [Required, MaxLength(30)]
        public string Type { get; set; }

        [Required]
        public bool IsOccupied { get; set; }

        // 1 Room - N Residents
        public ICollection<Resident> Residents { get; set; } = new List<Resident>();

        // 1 Room - N CheckInRecords
        public ICollection<CheckInRecord> CheckInRecords { get; set; } = new List<CheckInRecord>();
    }
}
