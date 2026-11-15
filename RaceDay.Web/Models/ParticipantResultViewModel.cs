
using System;

namespace RaceDay.Web.Models
{
    public class ParticipantResultViewModel
    {
        public int ResultId { get; set; }

        public int EnrolmentId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public TimeSpan? FinishTime { get; set; }

        public int? Position { get; set; }

        public int TotalFinishers { get; set; }

        public string ResultStatus { get; set; } = "Pending";
    }
}
