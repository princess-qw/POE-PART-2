
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Payments")]
    public class Payment
    {
        [Key]
        [Column("Payment_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }

        [Column("Enrolment_ID")]
        public int EnrolmentId { get; set; }

        [Column("Amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        [Column("Payment_Method")]
        public string? PaymentMethod { get; set; }

        [Required]
        [StringLength(20)]
        [Column("Payment_Status")]
        public string PaymentStatus { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("Transaction_Ref")]
        public string? TransactionRef { get; set; }

        [Column("Paid_At")]
        public DateTime? PaidAt { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Enrolment? Enrolment { get; set; }
    }
}
