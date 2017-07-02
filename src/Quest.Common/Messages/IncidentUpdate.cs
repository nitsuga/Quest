using System;

namespace Quest.Common.Messages
{
    /// <summary>
    /// This message is emitted when the number of simulated incidents drops below a certain threshold
    /// </summary>
    [Serializable]
    public class LowWaterIncidents : MessageBase
    {
        public long lastIncidentId;
    }

    /// <summary>
    /// indicatesthat a database update has occurred
    /// </summary>
    [Serializable]
    public class IncidentDatabaseUpdate : Request
    {
        public string serial;
        public EventMapItem Item;
    }


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
        public float? Latitude = 0;
        public float? Longitude = 0;
        public DateTime UpdateTime;
        public string UpdateType;

        public override string ToString()
        {
            return "IncidentUpdate " + Serial;
        }
    }
}