using System.ComponentModel.Composition;
using System.Linq;
using GeoAPI.Geometries;
using Quest.Lib.Constants;

namespace Quest.Lib.Routing.Speeds
{
    [Export("VariableSpeedHoWd", typeof(IRoadSpeedCalculator))]
    [Export(typeof(IRoadSpeedCalculator))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class VariableSpeedHoWd : IRoadSpeedCalculator
    {
        [Import]
        private SpeedMatrixLoader _speeddata;

        private readonly double _defaultSpeedMph = 22; // roughly 25 mph

        public RoadVector CalculateEdgeCost(string vehicletype, int hourOfWeek, RoadLinkEdge edge)
        {
            var vid = vehicletype == "AEU" ? 1 : 2;
            return CalculateEdgeCost(hourOfWeek, edge.RoadLinkEdgeId, edge.RoadTypeId, edge.Geometry.Coordinates.First(),
                vid, edge.Geometry.Length);
        }

        public int GetId()
        {
            return 2;
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