using System;

namespace Quest.Common.Messages.Incident
{
    [Serializable]
    public class IncidentUpdate : Request
    {
        public string Status ;
        public string Serial ;
        public string IncidentType ;
        public string Complaint ;
        public string Determinant ;
        public string Location ;
        public string Priority;
        public string LocationComment;
        public string PatientAge;
        public string PatientSex;
        /// <summary>
        /// DoH category
        /// </summary>
        public string Category ;
        
        public string Sector ;
        public string Description;
        public double? Latitude = 0;
        public double? Longitude = 0;
        public DateTime UpdateTime;
        public string UpdateType;

        public override string ToString()
        {
            return "IncidentUpdate " + Serial;
        }
    }
}