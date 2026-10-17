
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

        [Range(1, int.MaxValue,
            ErrorMessage = "A valid event ID is required.")]
        [Column("Event_ID")]
        public int EventId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [Column("Category_Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "1000.00",
            ErrorMessage = "Distance must be greater than zero and no more than 1000 km.")]
        [Column("Distance_Km", TypeName = "decimal(6,2)")]
        public decimal DistanceKm { get; set; }

        [Range(typeof(decimal), "0.00", "99999999.99",
            ErrorMessage = "Entry fee cannot be negative.")]
        [Column("Entry_Fee", TypeName = "decimal(10,2)")]
        public decimal EntryFee { get; set; }

        [Range(1, 1000000,
            ErrorMessage = "Maximum participation must be at least 1.")]
        [Column("Maximum_Participation")]
        public int MaximumParticipation { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Column("Start_Point")]
        public string StartPoint { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Column("End_Point")]
        public string EndPoint { get; set; } = string.Empty;

        [Range(0, int.MaxValue,
            ErrorMessage = "Elevation gain cannot be negative.")]
        [Column("Elevation_Gain_M")]
        public int? ElevationGainM { get; set; }

        [StringLength(255)]
        [Url(ErrorMessage = "Map URL must be a valid URL.")]
        [Column("Map_URL")]
        public string? MapUrl { get; set; }

        [Column("Created_At")]
        public DateTime CreatedAt { get; set; }

        public Event? Event { get; set; }
    }
}
