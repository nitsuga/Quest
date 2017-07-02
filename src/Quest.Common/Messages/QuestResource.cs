using GeoAPI.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quest.Common.Messages
{
    public class QuestResource
    {
        public bool Available;
        public int ResourceId;
        public long? Revision;
        public string Callsign;
        public ResourceStatus ResourceStatus;
        public ResourceStatus ResourceStatusPrev;
        public double Speed;
        public double Direction;
        public string Skill;
        public DateTime? LastUpdated;
        public int? FleetNo;
        public string Sector;
        public string Serial;
        public string Emergency;
        public string Agency;
        public string Class;
        public string EventType;
        public DateTime? ETA;
        public string Comment;
        public string Road;
        public Coordinate Position;
        public QuestDestination Destination;
    }
}
