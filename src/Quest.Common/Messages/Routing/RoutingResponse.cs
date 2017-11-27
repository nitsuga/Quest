using System;
using System.Collections.Generic;

namespace Quest.Common.Messages.Routing
{
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

}