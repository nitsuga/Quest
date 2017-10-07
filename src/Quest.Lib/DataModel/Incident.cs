using System;

namespace Quest.Lib.DataModel
{
    public partial class Incident
    {
        public int IncidentId { get; set; }
        public long? Revision { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string Status { get; set; }
        public string Serial { get; set; }
        public long? SerialNumber { get; set; }
        public string IncidentType { get; set; }
        public string Complaint { get; set; }
        public string Determinant { get; set; }
        public string DeterminantDescription { get; set; }
        public string Location { get; set; }
        public string Priority { get; set; }
        public string Sector { get; set; }
        public string Az { get; set; }
        public DateTime? Created { get; set; }
        public string LocationComment { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string ProblemDescription { get; set; }
        public DateTime? DisconnectTime { get; set; }
        public int? AssignedResources { get; set; }
        public bool? IsClosed { get; set; }
        public string CallerTelephone { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public int? CellX { get; set; }
        public int? CellY { get; set; }
    }
}
