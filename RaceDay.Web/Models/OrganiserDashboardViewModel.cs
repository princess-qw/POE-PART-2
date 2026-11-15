
using System;
using System.Collections.Generic;

namespace RaceDay.Web.Models
{
    public class OrganiserDashboardViewModel
    {
        public int TotalEvents { get; set; }

        public int UpcomingEvents { get; set; }

        public int TotalEnrolments { get; set; }

        public List<OrganiserDashboardEventViewModel> Events { get; set; }
            = new List<OrganiserDashboardEventViewModel>();
    }

    public class OrganiserDashboardEventViewModel
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;

        public DateTime EventDate { get; set; }

        public string EventLocation { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public string? BannerImage { get; set; }

        public int TotalEnrolments { get; set; }
    }
}
