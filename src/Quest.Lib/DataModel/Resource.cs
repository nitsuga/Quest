using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class Resource
    {
        public Resource()
        {
        }

        public int ResourceId { get; set; }
        public long? Revision { get; set; }
        public int? CallsignId { get; set; }
        public int? ResourceStatusId { get; set; }
        public double? SpeedMS { get; set; }
        public int? Direction { get; set; }
        public string Skill { get; set; }
        public string FleetNo { get; set; }
        public string Sector { get; set; }
        public string Incident { get; set; }
        public string Destination { get; set; }
        public string Agency { get; set; }
        public string EventType { get; set; }
        public DateTime? Eta { get; set; }
        public string Comment { get; set; }
        public int? ResourceTypeId { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? DestLatitude { get; set; }
        public float? DestLongitude { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public Callsign Callsign { get; set; }
        public ResourceStatus ResourceStatus { get; set; }
        public ResourceType ResourceType { get; set; }
    }
}
