using System;
using Quest.Common.Messages.GIS;

namespace Quest.Common.Messages.Incident
{
    public class QuestIncident
    {
        public long Revision { get; set; }
        public string Status { get; set; }
        public string EventId { get; set; }
        public string IncidentType { get; set; }
        public string Complaint { get; set; }
        public string Determinant { get; set; }
        public string DeterminantDescription { get; set; }
        public string Location { get; set; }
        public string Priority { get; set; }
        public string Sector { get; set; }
        public string LocationComment { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string ProblemDescription { get; set; }
        public DateTime? CallConnect{ get; set; }
        public DateTime? DisconnectTime { get; set; }
        public DateTime? DispatchTime { get; set; }
        public DateTime? FirstArrivalTime { get; set; }
        public int AssignedResources { get; set; }
        public bool? IsClosed { get; set; }
        public string CallerTelephone { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public LatLng Position { get; set; }
        public string Id { get; set; }
    }
}
