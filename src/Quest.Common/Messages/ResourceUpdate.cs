using System;

namespace Quest.Common.Messages
{

    /// <summary>
    /// notificate that a database change has occurred for this resource
    /// </summary>
    [Serializable]
    public class ResourceDatabaseUpdate : Request
    {
        public int ResourceId;

        // optional item details (missing if the item ha been deleted)
        public ResourceItem Item;
    }

    /// <summary>
    /// A resource update has been received from CAD
    /// </summary>
    [Serializable]
    public class ResourceUpdate : Request
    {
        public string Session;
        public string Callsign;
        public string ResourceType ;
        public string Status ;
        public double Latitude = 0;
        public double Longitude = 0;
        public int Speed ;
        public int Direction ;
        public string Skill ;
        public DateTime UpdateTime;
        public int FleetNo;
        public string Incident;
        public bool Emergency;
        public string Destination;
        public string Agency;
        public string Class;
        public string EventType;

        public override string ToString()
        {
            return $"ResourceUpdate {Callsign} Status={Status} type={ResourceType} event={Incident}";
        }
    }
}