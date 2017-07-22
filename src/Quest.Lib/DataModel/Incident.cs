//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Quest.Lib.DataModel
{
    using System;

    public partial class Incident
    {
        public int IncidentID { get; set; }
        public Nullable<long> Revision { get; set; }
        public Nullable<System.DateTime> LastUpdated { get; set; }
        public string Status { get; set; }
        public string Serial { get; set; }
        public Nullable<long> SerialNumber { get; set; }
        public string IncidentType { get; set; }
        public string Complaint { get; set; }
        public string Determinant { get; set; }
        public string DeterminantDescription { get; set; }
        public string Location { get; set; }
        public string Priority { get; set; }
        public string Sector { get; set; }
        public string AZ { get; set; }
        public Nullable<System.DateTime> Created { get; set; }
        public string LocationComment { get; set; }
        public string PatientSex { get; set; }
        public string PatientAge { get; set; }
        public string ProblemDescription { get; set; }
        public Nullable<System.DateTime> DisconnectTime { get; set; }
        public Nullable<int> AssignedResources { get; set; }
        public Nullable<bool> IsClosed { get; set; }
        public string CallerTelephone { get; set; }
        public Nullable<float> Latitude { get; set; }
        public Nullable<float> Longitude { get; set; }
    }
}
