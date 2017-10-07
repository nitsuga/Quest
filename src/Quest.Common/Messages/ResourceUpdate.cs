using System;

namespace Quest.Common.Messages
{

    /// <summary>
    /// notificate that a database change has occurred for this resource
    /// </summary>
    [Serializable]
    public class ResourceDatabaseUpdate : Request
    {
        public string Callsign;

        // optional item details (missing if the item ha been deleted)
        public ResourceItem Item;
    }

    /// <summary>
    /// A resource update has been received from CAD - this is a full update
    /// </summary>
    [Serializable]
    public class ResourceUpdate : MessageBase
    {
        public string Callsign;
        public string ResourceType ;
        public string Status ;
        public double Latitude = 0;
        public double Longitude = 0;
        public int Speed ;
        public int Direction ;
        public string Skill ;
        public string FleetNo;
        public string Incident;
        public bool Emergency;
        public string Destination;
        public string Agency;
        public string Class;
        public string EventType;
        public DateTime UpdateTime;

        public override string ToString()
        {
            return $"ResourceUpdate {Callsign} Status={Status} type={ResourceType} event={Incident}";
        }
    }
}