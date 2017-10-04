using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class ResourceItem: PointMapItem
    {
        public DateTime? lastUpdate;        
        public string Destination;        
        public DateTime? Eta;        
        public string FleetNo;        
        public string PrevStatus;        
        public string Road;        
        public string Skill;        
        public string Comment;        
        public int? Direction;        
        public int? Speed;        
        public string Incident;
        public bool Available;
        public bool Busy;
        public bool BusyEnroute;
        public bool PrevAvailable;
        public bool PrevBusy;
        public bool PrevBusyEnroute;

        // callsign        
        public string Callsign;

        /// <summary>
        ///     vehicle type
        /// </summary>
        public string VehicleType;

        // status
        public string Status;
        public string StatusCategory;
        public string ResourceTypeGroup;


    }

    
}