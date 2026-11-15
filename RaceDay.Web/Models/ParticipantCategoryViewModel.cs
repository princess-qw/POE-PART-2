
namespace RaceDay.Web.Models
{
    public class ParticipantCategoryViewModel
    {
        public int CategoryId { get; set; }

        public int EventId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public decimal EntryFee { get; set; }

        public int MaximumParticipation { get; set; }

        public string StartPoint { get; set; } = string.Empty;

        public string EndPoint { get; set; } = string.Empty;
    }
}
