using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
    [Serializable]
    
    public class RoutingEngineStatusRequest : Request
    {
    }

    
    [Serializable]
    public class RoutingEngineStatus : Response
    {
        
        public bool Ready;
    }

    /// <summary>
    ///     This class contains a list of results from a routing engine search. each RoutingResult contains
    ///     the endpoint found by the routing engine along with the track taken to reach it and the
    ///     duration it would take the vehicle to reach the endpoint.
    /// </summary>
    
    [Serializable]
    public class RoutingResponse : Response, IRouteResult
    {
        
        public List<RoutingResult> Items = new List<RoutingResult>();

    }

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

    
    [Serializable]
    public class Waypoint
    {
        public double X;
        public double Y;
    }

}