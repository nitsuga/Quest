using System;

namespace Quest.Common.Messages
{
    public partial class QuestIncident
    {
        public int IncidentID { get; set; }
        public long? Revision { get; set; }
        public string Status { get; set; }
        public string Serial { get; set; }
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
        public DateTime? StartDate;
        public DateTime? EndDate;
    }
}
