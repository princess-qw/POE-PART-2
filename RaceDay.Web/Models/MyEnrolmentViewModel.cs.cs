using System;

namespace RaceDay.Web.Models
{
    public class MyEnrolmentViewModel
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string BibNumber { get; set; } = string.Empty;

        public ParticipantCategoryViewModel? Category { get; set; }
    }
}