
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

        [Range(1, int.MaxValue, ErrorMessage = "A valid organiser is required.")]
        [Column("Organiser_ID")]
        public int OrganiserId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Event name must be between 3 and 100 characters.")]
        public string Event_Name { get; set; } = string.Empty;

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime Event_Date { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3,
            ErrorMessage = "Event location must be between 3 and 100 characters.")]
        public string Event_Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Event type must be between 3 and 50 characters.")]
        public string Event_Type { get; set; } = string.Empty;

        [StringLength(255,
            ErrorMessage = "Description cannot exceed 255 characters.")]
        public string? Description { get; set; }

        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime Registration_Deadline { get; set; }

        [Column("Created_At")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; }

        public Organiser? Organiser { get; set; }
    }
}
