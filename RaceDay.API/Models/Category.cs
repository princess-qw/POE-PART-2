
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.API.Models
{
    [Table("Categories")]
    public class Category
    {
        [Key]
        [Column("Category_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Column("Event_ID")]
        public int EventId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Category_Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Column("Distance_Km", TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        [Column("Entry_Fee", TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        [Column("Maximum_Participation")]
        public int MaximumParticipation { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Start_Point")]
        public string StartPoint { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("End_Point")]
        public string EndPoint { get; set; } = string.Empty;

        [Column("Elevation_Gain_M")]
        public int? ElevationGainM { get; set; }

        [StringLength(255)]
        [Column("Map_URL")]
        public string? MapUrl { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Event? Event { get; set; }
    }
}
