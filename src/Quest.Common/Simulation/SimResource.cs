using System;
using GeoAPI.Geometries;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class SimResource : QuestResource
    {
        public int GPSFrequency;
        public string VehicleType;
        public bool PoweredOn;
        public bool OnDuty;
        public ResourceStatus Status;
        public new SimDestination Destination;  // overrides the QuestDestination Object
        public SimDestination StandbyPoint;
        public double TTG;
        public double DTG;
        public DateTime LastChanged;
        public SimIncident Incident;
        public int NonConveyCode;
        public Coordinate RoutingPoint;
        public double Heading;
        public string Location;
        public int AtDestinationRange;
        public bool IncidentAccept;
        public string DestHospital;
        public string SysMessage;
        public string LastSysMessage;
        public string TxtMessage;
        public object LastTxtMessage;
        public string MdtMessage;
        public string LastMdtMessage;
        public object LastMessagePriority;
        public bool Enabled;
        public bool AcceptCancellation;
        public int IncidentResponseSpeed;
    }

}
