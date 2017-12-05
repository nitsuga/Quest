using System;
using System.Collections.Generic;
using Quest.Lib.Routing;

namespace Quest.Lib.OS.Routing.Highways
{
    [Serializable]
    public class RoutingLocation : RoutingPoint
    {
        /// <summary>
        ///     unique id of this location
        /// </summary>
        public int Id = 0;

        public List<RoadLinkEdgeTemp> OutEdges = new List<RoadLinkEdgeTemp>();

        public RoutingLocation(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"{(int)X} {(int)Y} ({Id})";
    }
}