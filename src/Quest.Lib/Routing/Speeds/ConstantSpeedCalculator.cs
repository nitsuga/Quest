using Quest.Common.Messages;
using Quest.Common.Messages.Routing;
using Quest.Lib.Trace;

namespace Quest.Lib.Routing.Speeds
{
    public class ConstantSpeedCalculator : IRoadSpeedCalculator
    {
        public double defaultSpeedMs { get; set; } = 13.2; // 29.53 mph

        public bool debug { get; set; }

        public RoadVector CalculateEdgeCost(string vehicletype, int hour, RoadEdge edge)
        {
            if (debug)
                Logger.Write($"vtype: {vehicletype}, hour: {hour} edge: {edge.RoadName} len: {edge.Length} spd: {defaultSpeedMs}", GetType().Name);

            return new RoadVector
            {
                DistanceMeters = edge.Length,
                DurationSecs = edge.Length/defaultSpeedMs,
                SpeedMs = defaultSpeedMs
            };
        }

        public int GetId()
        {
            return 10;
        }
    }
}