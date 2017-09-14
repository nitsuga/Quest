using System;
using Quest.Common.Messages;

namespace Quest.WebCore.Services
{
    [Serializable]
    public class RouteInfo : WorkItem
    {
        public int FromX;
        public int FromY;
        public int ToX;
        public int ToY;
        public int Hour;
        public string Vehicle;
        public double ActualTimeSecs;
        public double EstTimeSecs;
        public string RoadSpeedCalculator;

        [NonSerialized]
        public RouteRequest Request;

        [NonSerialized]
        public RoutingResponse Result;

    }
}