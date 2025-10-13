using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SORMS.API.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public DateTime GeneratedDate { get; set; }

        [Required]
        public string Content { get; set; }

        [Required, MaxLength(100)]
        public string CreatedBy { get; set; }
    }
}
