using System.Linq;
using GeoAPI.Geometries;
using Quest.Common.Messages;
using Quest.Lib.Constants;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing.Speeds
{
    public class VariableSpeedHoW : IRoadSpeedCalculator
    {
        
        private SpeedDataHoW _speeddata;

        private readonly double _defaultSpeedMph = 22; // roughly 25 mph
        public VariableSpeedHoW()
        {
        }

        public VariableSpeedHoW(SpeedDataHoW speeddata)
        {
            _speeddata = speeddata;
        }

        public RoadVector CalculateEdgeCost(string vehicletype, int hourOfWeek, RoadEdge edge)
        {
            var vid = vehicletype == "AEU" ? 1 : 2;
            return CalculateEdgeCost(hourOfWeek, edge.RoadLinkEdgeId, edge.RoadTypeId, edge.Geometry.Coordinates.First(),
                vid, edge.Geometry.Length);
        }

        public int GetId()
        {
            return 4;
        }

        private RoadVector CalculateEdgeCost(int hourOfWeek, int roadLinkId, int roadTypeId, Coordinate coord, int vid,
            double length)
        {
            double speed = _speeddata.GetRoadSpeedMphHoW(roadTypeId, coord, vid, hourOfWeek);

            if (speed <= 0)
                speed = _defaultSpeedMph;

            var speedms = speed*Constant.mph2ms;

            return new RoadVector
            {
                DistanceMeters = length,
                DurationSecs = length/ speedms,
                SpeedMs = speedms
            };
        }
    }
}