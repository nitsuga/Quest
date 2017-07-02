using System;
using GeoAPI.Geometries;

namespace Quest.Common.Simulation
{
    public class VehicleRoster
    {
        public string Callsign;
        public string VehicleType;
        public DateTime StartTime;
        public TimeSpan Duration;
        public Coordinate StartPosition;
    }

}
