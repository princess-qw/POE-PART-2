
using System;

namespace RaceDay.Web.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }

        public int ParticipantId { get; set; }

        public int CategoryId { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public string BibNumber { get; set; } = string.Empty;

        public ParticipantEnrolmentViewModel? Participant { get; set; }

        public CategoryEnrolmentViewModel? Category { get; set; }
    }

    public class ParticipantEnrolmentViewModel
    {
        public int ParticipantId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public class CategoryEnrolmentViewModel
    {
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public decimal DistanceKm { get; set; }
    }
}
