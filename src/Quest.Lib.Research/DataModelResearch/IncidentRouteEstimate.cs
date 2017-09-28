namespace Quest.Lib.Research.DataModelResearch
{
    public partial class IncidentRouteEstimate
    {
        public int IncidentRouteEstimateId { get; set; }
        public int RoutingMethod { get; set; }
        public int EdgeMethod { get; set; }
        public int EstimatedDuration { get; set; }
        public int IncidentRouteId { get; set; }

        public IncidentRoutes IncidentRoute { get; set; }
    }
}
