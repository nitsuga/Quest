using GeoAPI.Geometries;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    public class AtDestination:MessageBase
    {
        public string Callsign;
        public int ConveyCode;
        public DestType DestType;
        public double Easting;
        public double Northing;
        public string EventCode;
        public Coordinate Position;
    }

}
