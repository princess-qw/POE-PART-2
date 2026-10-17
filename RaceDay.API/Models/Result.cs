
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

        [Column("Enrolment_ID")]
        public int EnrolmentId { get; set; }

        [Column("Finish_Time", TypeName = "time")]
        public TimeSpan? FinishTime { get; set; }

        [Column("Position")]
        public int? Position { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Result_Status")]
        public string ResultStatus { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}
