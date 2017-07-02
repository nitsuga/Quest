using System;

namespace Quest.Common.Messages
{
    [Serializable]
    
    public class ResourceItem
    {
        public long revision;        
        public DateTime? lastUpdate;        
        public string Destination;        
        public DateTime? Eta;        
        public int? FleetNo;        
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

        public float? Latitude;
        public float? Longitude;
        public int ID { get; set; }        
        public double Y { get; set;}        
        public double X { get; set; }

        // callsign
        
        public string Callsign { get; set; }

        /// <summary>
        ///     vehicle type
        /// </summary>
        
        public string VehicleType { get; set; }

        // status
        
        public string Status { get; set; }

        
        public string StatusCategory { get; set; }
        
        public string ResourceTypeGroup { get; set; }

        
    }

    
}