
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("EventSponsors")]
    public class EventSponsor
    {
        [Key]
        [Column("Event_Sponsor_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventSponsorId { get; set; }

        [Column("Event_ID")]
        public int EventId { get; set; }

        [Column("Sponsor_ID")]
        public int SponsorId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Sponsorship_Tier")]
        public string SponsorshipTier { get; set; } = string.Empty;

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Event? Event { get; set; }

        public Sponsor? Sponsor { get; set; }
    }
}
