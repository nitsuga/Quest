using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class Resource
    {
        public Resource()
        {
            Devices = new HashSet<Devices>();
            ResourceStatusHistory = new HashSet<ResourceStatusHistory>();
        }

        public int ResourceId { get; set; }
        public long? Revision { get; set; }
        public int? CallsignId { get; set; }
        public int? ResourceStatusId { get; set; }
        public int? ResourceStatusPrevId { get; set; }
        public int? Speed { get; set; }
        public int? Direction { get; set; }
        public string Skill { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int? FleetNo { get; set; }
        public string Sector { get; set; }
        public string Serial { get; set; }
        public string Emergency { get; set; }
        public string Destination { get; set; }
        public string Agency { get; set; }
        public string Class { get; set; }
        public string EventType { get; set; }
        public DateTime? Eta { get; set; }
        public string Comment { get; set; }
        public string Road { get; set; }
        public int? ResourceTypeId { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? DestLatitude { get; set; }
        public float? DestLongitude { get; set; }

        public Callsign Callsign { get; set; }
        public ResourceStatus ResourceStatus { get; set; }
        public ResourceStatus ResourceStatusPrev { get; set; }
        public ResourceType ResourceType { get; set; }
        public ICollection<Devices> Devices { get; set; }
        public ICollection<ResourceStatusHistory> ResourceStatusHistory { get; set; }
    }
}
