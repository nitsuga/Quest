using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing.Speeds
{

    public abstract class RoadTypeSpeedCalculator : IRoadSpeedCalculator
    {
        private double _defaultSpeedMs = 0; // roughly 25 mph
        private double _junctionDelaySecs = 0;
        private double[] _speeds = new double[] { };
        private int _id=0;

        protected void Initialise(int id, double defaultSpeedMs, double junctionDelaySecs, double[] speeds)
        {
            _speeds = speeds;
            _defaultSpeedMs = defaultSpeedMs;
            _junctionDelaySecs = junctionDelaySecs;
            _id = id;
        }

        public RoadVector CalculateEdgeCost(string vehicletype, int hour, RoadEdge edge)
        {
            var secs = _junctionDelaySecs + (edge.Geometry.Length / _speeds[edge.RoadTypeId - 1]);
            return new RoadVector
            {
                DistanceMeters = edge.Geometry.Length,
                DurationSecs = edge.Geometry.Length / _speeds[edge.RoadTypeId - 1],
                SpeedMs = _defaultSpeedMs
            };
        }

        public int GetId()
        {
            return _id;
        }
    }

    public class LASRoadTypeSpeedCalculator : RoadTypeSpeedCalculator
    {
        public LASRoadTypeSpeedCalculator()
        {
            Initialise(21, 11, 2.5, new double[] { 29, 3, 24, 14, 19, 35, 2, 5, 5 });
        }
    }

    public class GPSRoadTypeSpeedCalculator : RoadTypeSpeedCalculator
    {
        public GPSRoadTypeSpeedCalculator()
        {
            Initialise(22, 11, 0, new double[] { 20.74, 14.61, 18.01, 10.89, 16.69, 38.79, 8.12, 8.14, 10.55 });
        }
    }

    public class HMMVRoadTypeSpeedCalculator : RoadTypeSpeedCalculator
    {
        public HMMVRoadTypeSpeedCalculator()
        {
            Initialise(23, 11, 0, new double[] { 31.21, 22.51, 28.01, 18.79, 26.8, 42.65, 16.29, 16.58, 18.50 });
        }
    }
}