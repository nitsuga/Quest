using System;
using System.Collections.Generic;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class IncidentRoutes
    {
        public IncidentRoutes()
        {
            IncidentRouteEstimate = new HashSet<IncidentRouteEstimate>();
        }

        public int IncidentRouteId { get; set; }
        public string Callsign { get; set; }
        public long? IncidentId { get; set; }
        public bool? Scanned { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? ActualDuration { get; set; }
        public bool? IsBadGps { get; set; }

        public ICollection<IncidentRouteEstimate> IncidentRouteEstimate { get; set; }
    }
}
