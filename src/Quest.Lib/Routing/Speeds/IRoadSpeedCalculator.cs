using Quest.Common.Messages;

namespace Quest.Lib.Routing.Speeds
{
    public interface IRoadSpeedCalculator
    {
        RoadVector CalculateEdgeCost(string vehicletype, int hourOfWeek, RoadEdge edge);

        int GetId();
    }
}
