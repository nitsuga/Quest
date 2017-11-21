using System.Collections.Generic;
using Quest.Lib.Routing;
using Quest.Common.Messages.GIS;

namespace Quest.Lib.MapMatching.RouteMatcher
{
    public interface IMapMatchParameters
    {
    }

    /// <summary>
    /// A single request to perform map matching on a given route
    /// </summary>
    public class RouteMatcherRequest
    {
        public dynamic Parameters;

        public string Name;

        public string RoadSpeedCalculator;

        public RoutingData RoutingData;

        /// <summary>
        ///     routing engine to use (if any)
        /// </summary>
        public IRouteEngine RoutingEngine;

        /// <summary>
        ///  The observations to create a route from
        /// </summary>
        ///public Track Track;

        public List<Fix> Fixes;
    }
}