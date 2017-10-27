using Quest.Common.Messages.Routing;
using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.GIS
{
    [Serializable]
    public class MapMatcherMatchAllRequest : Request
    {
        public bool InProcess;

        public int Workers;

        public string RoutingEngine = "Dijkstra";

        public string MapMatcher = "ParticleFilterMapMatcher";

        public string RoutingData = "Standard";

        public string Parameters;
        public string Components;

        public override string ToString()
        {
            return "MapMatcherMatchAll";
        }
    }

    [Serializable]
    public class MapMatcherMatchAllResponse : Response
    {
    }

    [Serializable]
    public class MapMatcherMatchSingleRequest : Request
    {
        public string RoutingData = "Standard";

        public string RoutingEngine = "Dijkstra";

        public string MapMatcher = "ParticleFilterMapMatcher";

        public string Name = "unknown";

        public List<Fix> Fixes;

        public dynamic Parameters;

        public override string ToString()
        {
            return "MapMatcherMatchSingleRequest";
        }
    }

    [Serializable]
    public class MapMatcherMatchSingleResponse : Response
    {
        public RouteMatcherResponse Result;

        public override string ToString()
        {
            return "MapMatcherMatchAll";
        }
    }

}