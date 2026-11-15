
using System;

namespace RaceDay.Web.Models
{
    public class ResultsViewModel
    {
        public int EnrolmentId { get; set; }

        public int ResultId { get; set; }

        public string ParticipantName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }

        public string BibNumber { get; set; } = string.Empty;

        public string? FinishTime { get; set; }

        public int? Position { get; set; }

        public string ResultStatus { get; set; } = "Pending";
    }
}
