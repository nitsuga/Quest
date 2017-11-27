using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{

    /// <summary>
    ///     a single routing result
    /// </summary>

    [Serializable]
    public class RoutingResult
    {        
        public List<RoadEdgeWithVector> Connections;
        
        public Waypoint[] PathPoints;
        
        public double Distance;
        
        public double Duration;

        /// <summary>
        /// The target end edge that was requested.
        /// </summary>
        
        public EdgeWithOffset EndEdge;
        
    }

}