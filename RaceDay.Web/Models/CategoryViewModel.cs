
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Required]
        [Range(typeof(decimal), "0.01", "1000.00")]
        [Display(Name = "Distance (KM)")]
        public decimal DistanceKm { get; set; }

        [Required]
        [Range(typeof(decimal), "0.00", "99999999.99")]
        [Display(Name = "Entry Fee")]
        public decimal EntryFee { get; set; }

        [Required]
        [Range(1, 1000000)]
        [Display(Name = "Maximum Participation")]
        public int MaximumParticipation { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Start Point")]
        public string StartPoint { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "End Point")]
        public string EndPoint { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        [Display(Name = "Elevation Gain (M)")]
        public int? ElevationGainM { get; set; }

        [StringLength(255)]
        [Url]
        [Display(Name = "Map URL")]
        public string? MapUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
