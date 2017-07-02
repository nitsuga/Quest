using System.Collections.Generic;
using Quest.Common.Messages;

namespace Quest.Lib.Routing
{
    internal class RoutingEndPoint
    {
        /// <summary>
        /// The original position to route to
        /// </summary>
        internal List<EdgeWithOffset> Originals;

        /// <summary>
        /// the target edge containing the original position
        /// </summary>
        //internal RoadLinkEdge Edge;

        /// <summary>
        /// The closest real node to route to
        /// </summary>
        internal RoadEdge Normalised;
    }
}