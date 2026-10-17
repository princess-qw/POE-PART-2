
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Events")]
    public class Event
    {
        [Key]
        [Column("Event_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        [Column("Organiser_ID")]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Event_Name { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        public DateTime Event_Date { get; set; }

        [Required]
        [StringLength(100)]
        public string Event_Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Event_Type { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        [Column(TypeName = "date")]
        public DateTime Registration_Deadline { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Organiser? Organiser { get; set; }
    }
}
