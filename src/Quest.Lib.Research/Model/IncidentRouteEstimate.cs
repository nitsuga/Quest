//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.Research.Model
{

    public partial class IncidentRouteEstimate
    {
        public int IncidentRouteEstimateId { get; set; }
        public int EstimatedDuration { get; set; }
        public int IncidentRouteId { get; set; }
        public System.Data.Entity.Spatial.DbGeometry EstimateRoute { get; set; }
        public int RoutingMethod { get; set; }
        public int EdgeMethod { get; set; }
    
        public virtual IncidentRoute IncidentRoute { get; set; }
    }
}
