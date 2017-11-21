using Quest.Common.Messages.Routing;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     class hold routing engine data as a route is being calculated. instance of this class are
    ///     destroyed after a route has been claculated
    /// </summary>
    public class RoutingEdgeData
    {
        /// <summary>
        ///     is this a target end point
        /// </summary>
        public bool IsEndpoint;

        public RoadEdge Edge;

        /// <summary>
        ///     The previous location in the route
        /// </summary>
        public RoadEdge PreviousEdge;

        /// <summary>
        ///     has this node been processed yet
        /// </summary>
        public bool Processed;

        /// <summary>
        ///     distance to this location in meters - ONLY USED DURING ROUTING
        /// </summary>
        public double RouteDistance;

        /// <summary>
        ///     time in seconds to this location - ONLY USED DURING ROUTING
        /// </summary>
        public double RouteDuration;

        /// <summary>
        /// Vector returned by the speed calculator
        /// </summary>
        public RoadVector Vector;

        public RoutingEdgeData()
        {
            Processed = false;
            RouteDistance = double.MaxValue;
            RouteDuration = double.MaxValue;
            PreviousEdge = null;
            IsEndpoint = false;
        }

    }
}