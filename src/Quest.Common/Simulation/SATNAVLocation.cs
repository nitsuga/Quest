using GeoAPI.Geometries;
using Quest.Common.Messages;

namespace Quest.Common.Simulation
{
    /// <summary>
    /// transmitted when vehicle changes position
    /// </summary>
    public class SATNAVLocation: MessageBase
    {
        public string Callsign;
        public Coordinate Position;
        public double EtaDistance;
        public double EtaSeconds;
        public double Direction;
        public double Speed;
    }

}
