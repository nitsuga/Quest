using GeoAPI.Geometries;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class StatusChange:MessageBase
    {
        public int ResourceId;
        public ResourceStatus Status;
        public int NonConveyCode;
        public string DestHospital;
        public Coordinate Position;
    }

}
