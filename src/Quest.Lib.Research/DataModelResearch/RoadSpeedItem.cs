using System;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class RoadSpeedItem
    {
        public int RoadSpeedItemId { get; set; }
        public int IncidentRouteRunId { get; set; }
        public int IncidentRouteId { get; set; }
        public DateTime DateTime { get; set; }
        public double Speed { get; set; }
        public int? RoadLinkEdgeId { get; set; }

        public IncidentRoutes IncidentRoute { get; set; }
    }
}
