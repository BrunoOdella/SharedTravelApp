using System;
using System.Collections.Generic;

namespace StatisticsServer.DTO
{
    public class TripReport
    {
        public Guid ReportId { get; set; }
        public List<Trip> Trips { get; set; } = new List<Trip>();
        public bool IsReady { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadyAt { get; set; }
        public int RequiredTrips { get; set; }

        public TripReport(int requiredTrips)
        {
            ReportId = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            RequiredTrips = requiredTrips;
        }
    }
}
