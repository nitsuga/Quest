using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Common.Messages.Resource;

namespace Quest.Common.Simulation
{
    public class StatusChange:MessageBase
    {
        public string Callsign;
        public ResourceStatus Status;
        public int NonConveyCode;
        public string DestHospital;
        public Coordinate Position;
    }

}
