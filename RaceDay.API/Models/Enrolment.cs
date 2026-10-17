
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Enrolments")]
    public class Enrolment
    {
        [Key]
        [Column("Enrolment_ID")]
        public int EnrolmentId { get; set; }

        [Required]
        [Column("Participant_ID")]
        public int ParticipantId { get; set; }

        [Required]
        [Column("Category_ID")]
        public int CategoryId { get; set; }

        [Column("Enrolment_Date")]
        public DateTime EnrolmentDate { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";

        [Required]
        [StringLength(20)]
        [Column("Bib_Number")]
        public string BibNumber { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Participant? Participant { get; set; }

        public Category? Category { get; set; }
    }
}
