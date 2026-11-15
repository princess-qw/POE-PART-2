
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Web.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }

        public int OrganiserId { get; set; }

        [Required(ErrorMessage = "Event name is required.")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Event Name")]
        public string Event_Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Event Date")]
        public DateTime Event_Date { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Event location is required.")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Event Location")]
        public string Event_Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Event type is required.")]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Event Type")]
        public string Event_Type { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Registration deadline is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Registration Deadline")]
        public DateTime Registration_Deadline { get; set; } = DateTime.Today;


        public string? BannerImage { get; set; }

        public IFormFile? BannerFile { get; set; }

    }
}
