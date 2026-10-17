
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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EnrolmentId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A valid participant ID is required.")]
        [Column("Participant_ID")]
        public int ParticipantId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A valid category ID is required.")]
        [Column("Category_ID")]
        public int CategoryId { get; set; }

        [Column("Enrolment_Date", TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime EnrolmentDate { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(
            "^(Pending|Confirmed|Cancelled)$",
            ErrorMessage = "Status must be Pending, Confirmed, or Cancelled.")]
        public string Status { get; set; } = "Pending";

        [Required]
        [StringLength(20, MinimumLength = 1)]
        [RegularExpression(
            @"^[A-Za-z0-9-]+$",
            ErrorMessage = "Bib number may contain only letters, numbers, and hyphens.")]
        [Column("Bib_Number")]
        public string BibNumber { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Participant? Participant { get; set; }

        public Category? Category { get; set; }
    }
}
