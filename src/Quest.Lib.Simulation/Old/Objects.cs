using System;
using Quest.Lib.Routing;

namespace Quest.Lib.Simulation
{
    [Serializable]
    public class SimResource
    {
        public string Callsign;
        public RoutingPoint location;
        public string Status;
        public string Type;
    }

    [Serializable]
    public class SimIncident
    {
        public RoutingPoint location;
        public string Priority;
        public string Serial;
        public string Status;
    }
}