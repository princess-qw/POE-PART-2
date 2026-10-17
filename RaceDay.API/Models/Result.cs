
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Results")]
    public class Result
    {
        [Key]
        [Column("Result_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultId { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "A valid enrolment ID is required.")]
        [Column("Enrolment_ID")]
        public int EnrolmentId { get; set; }

        [Column("Finish_Time", TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        [Range(1, int.MaxValue,
            ErrorMessage = "Position must be greater than zero.")]
        [Column("Position")]
        public int? Position { get; set; }

        [Required]
        [StringLength(20)]
        [RegularExpression(
            "^(Pending|Finished|DNF|DSQ)$",
            ErrorMessage = "Status must be Pending, Finished, DNF, or DSQ.")]
        [Column("Result_Status")]
        public string ResultStatus { get; set; } = "Pending";

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}
