
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Sponsors")]
    public class Sponsor
    {
        [Key]
        [Column("Sponsor_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SponsorId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Sponsor_Name")]
        public string SponsorName { get; set; } = string.Empty;

        [StringLength(100)]
        [Column("Contact_Person")]
        public string? ContactPerson { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Sponsor_Type")]
        public string SponsorType { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }
    }
}
