#pragma warning disable 0169
#pragma warning disable 0649

using Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using Quest.Lib.Routing.Speeds;
using Quest.Lib.Utils;
using Quest.Lib.Trace;
using Quest.Common.Messages.Routing;
using Quest.Lib.Routing.Coverage;

namespace Quest.Lib.Routing
{
    /// <summary>
    ///     This is an implementation of the Dijkstra's algorithm for routing.It can be used to determine
    ///     paths from a point to one or more endpoints. Limits can be placed on how far the search is made
    ///     along the road network.
    /// </summary>

    public class DijkstraRoutingEngine : IRouteEngine, IPart
    {
        /// <summary>
        ///     contains the road network
        /// </summary>
        private RoutingData _data;
        private ILifetimeScope _scope;

        public List<int> IgnoreRoadTypes { get; set; }

        public DijkstraRoutingEngine(RoutingData data, ILifetimeScope scope)
        {
            _data = data;
            _scope = scope;
        }

        public RoutingData Data => _data;

        public bool IsReady => _data.IsInitialised;

        public RoutingResponse CalculateRouteMultiple(RouteRequestMultiple request)
        {

            // wait until ready
            while (!IsReady)
                Thread.Sleep(250);

            if (request == null) throw new ArgumentNullException(nameof(request));

            IRoadSpeedCalculator speedCalc = _scope.ResolveNamed<IRoadSpeedCalculator>(request.RoadSpeedCalculator);

            if (speedCalc == null)
                return new RoutingResponse
                {
                    Message = $"Can't find speed calculator {request.RoadSpeedCalculator}",
                    Success = false
                };

            return CalculateRouteMultiple(request, speedCalc);
        }

        /// <summary>
        ///     calculate the route to multiple targets
        /// </summary>
        /// <returns></returns>
        public RoutingResponse CalculateRouteMultiple(RouteRequestMultiple request, IRoadSpeedCalculator speedCalc)
        {
            try
            {
                //Logger.Write($"Request: road links: {_data.ConnectionIndex.Count} calculator: {request.RoadSpeedCalculator} end points: {request.EndLocations.Count}", TraceEventType.Information, this.GetType().Name);
                //Logger.Write($"Request: start  id: {request.StartLocation.Edge.RoadLinkEdgeId} name: {request.StartLocation.Edge.RoadName}", TraceEventType.Information, this.GetType().Name);

                var results = new RoutingResponse();

                // create a coverage map if one has been asked for.
                CoverageMap localmap = null;
                if (request.Map != null)
                    localmap = CoverageMapUtil.CreateEmptyCopy(request.Map);

                // remember which edges have been visited
                var visited = new Dictionary<int, RoutingEdgeData>();
                var candidates = new Dictionary<int, RoadEdge>();
                var candidatesQuick = new FibonacciHeap<double, RoadEdge>();

                // set all location weights to maxint, 
                // candidates will have just one location - the start location
                // the end location has a flag to say its the target destination
                var originalEndcount = request.EndLocations?.Count ?? 0;

                ////Logger.Write($"Request: end point count: {originalEndcount}", TraceEventType.Information, this.GetType().Name);
                //foreach (var e in request.EndLocations)
                //    //Logger.Write($"         end point: {e.Edge.RoadLinkEdgeId} {e.Edge.RoadName} {e.Coord.X}/{e.Coord.Y}", TraceEventType.Information, this.GetType().Name);

                var normalisedEndpoints = InitialiseRoute(speedCalc, request, request.StartLocation, request.EndLocations, results, ref candidates, ref candidatesQuick, ref visited);
                //ogger.Write($"Normalised end point count: {normalisedEndpoints.Count}", TraceEventType.Information, this.GetType().Name);

                if (request.EndLocations != null && (normalisedEndpoints.Count == 0 && request.EndLocations.Count > 0 && results.Items.Count > 0))
                {
                    results.Message = "No end locations could be coerced to a road link";
                    //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                    return results;
                }

                if (request.EndLocations != null && request.EndLocations.Count == 0 && originalEndcount > 0)
                {
                    results.Message = "No end locations either different from start position or could be coerced to a road link";
                    //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                    return results;
                }

                while (true)
                {
                    // get Edge with the minimum weight
                    RoadEdge minEdge = null;
                    RoutingEdgeData minEdgeData = null;

                    var m = candidatesQuick.Top();
                    if (m != null)
                    {
                        minEdge = m;
                        minEdgeData = visited[minEdge.RoadLinkEdgeId];
                    }


                    // we reached our distance limit or ran out of locations
                    if (minEdgeData == null || minEdgeData.RouteDistance > request.DistanceMax)
                    {
                        // add map locations
                        if (localmap != null && request.Map != null)
                            request.Map?.Add(localmap);
                        results.Message = $"Reached distance limit of {request.DistanceMax}";
                        //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                        return results;
                    }

                    // we reached our distance limit or ran out of locations
                    if (minEdgeData == null)
                    {
                        results.Message = $"minEdge is NULL";
                        //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                        return results;
                    }

                    // we reached our duration limit
                    if (minEdgeData.RouteDuration > request.DurationMax)
                    {
                        request.Map?.Add(localmap);
                        results.Message = "Reached time limit";
                        //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                        return results;
                    }

                    // we found one of the destinations we are looking for, record the route and return if we 
                    // have captured all of them
                    if (minEdgeData.IsEndpoint)
                    {
 
                        AddResult(normalisedEndpoints, minEdgeData, results, request, visited);
                        
                        // if we found all the endpoints return the result
                        if (request.EndLocations != null &&
                            (results.Items.Count == request.EndLocations.Count || results.Items.Count >= request.InstanceMax))
                        {
                            if (request.Map != null)
                                ProcessLocations(request.Map, visited);

                            results.Message = $"Found {request.EndLocations.Count} end points, {results.Items.Count} routes found";

                            //Logger.Write(results.Message, TraceEventType.Information, this.GetType().Name);
                            return results;
                        }
                    }

                    ////Logger.Write($"7", TraceEventType.Information, this.GetType().Name);

                    minEdgeData.Processed = true;

                    candidates.Remove(minEdge.RoadLinkEdgeId);

                    candidatesQuick.Dequeue();

                    // update local coverage map
                    //TODO:
                    if (request.Map != null)
                        localmap.Set((int)minEdge.Geometry.Centroid.X, (int)minEdge.Geometry.Centroid.Y, 1);

                    var grade = (minEdgeData.Edge?.TargetGrade) ?? 0;

                    // process each edge of the vertex and relax the distance/time
                    foreach (var edge in minEdge.OutEdges)
                    {
                        if (edge.SourceGrade != grade)
                            continue;

                        // ignore road types we're not interested in.
                        if (IgnoreRoadTypes!=null && IgnoreRoadTypes.Contains(edge.RoadTypeId))
                            continue;

                        // create an entry if it doesn't exist
                        RoutingEdgeData thisEdgeData;

                        if (!visited.ContainsKey(edge.RoadLinkEdgeId))
                        {
                            thisEdgeData = new RoutingEdgeData { Edge = edge };
                            visited.Add(edge.RoadLinkEdgeId, thisEdgeData);
                        }
                        else
                            thisEdgeData = visited[edge.RoadLinkEdgeId];

                        // for this connection work out how fast the vehicle travels down it
                        if (!thisEdgeData.Processed)
                        {
                            var vec = speedCalc.CalculateEdgeCost(request.VehicleType, request.HourOfWeek, edge);
                            var newDistance = vec.DistanceMeters + minEdgeData.RouteDistance;
                            var newDuration = vec.DurationSecs + minEdgeData.RouteDuration;

                            switch (request.SearchType)
                            {
                                case RouteSearchType.Shortest:

                                    if (thisEdgeData.RouteDistance >= newDistance)
                                    {
                                        thisEdgeData.RouteDistance = newDistance;
                                        thisEdgeData.RouteDuration = newDuration;
                                        thisEdgeData.PreviousEdge = minEdge;
                                        thisEdgeData.Edge = edge;
                                        thisEdgeData.Vector = vec;

                                        if (!candidates.ContainsKey(edge.RoadLinkEdgeId))
                                        {
                                            candidates.Add(edge.RoadLinkEdgeId, edge);
                                            candidatesQuick.Enqueue(thisEdgeData.RouteDistance, edge);
                                        }
                                    }
                                    break;

                                case RouteSearchType.Quickest:
                                    if (thisEdgeData.RouteDuration >= newDuration)
                                    {
                                        thisEdgeData.RouteDuration = newDuration;
                                        thisEdgeData.RouteDistance = newDistance;
                                        thisEdgeData.PreviousEdge = minEdge;
                                        thisEdgeData.Edge = edge;
                                        thisEdgeData.Vector = vec;

                                        if (!candidates.ContainsKey(edge.RoadLinkEdgeId))
                                        {
                                            candidates.Add(edge.RoadLinkEdgeId, edge);
                                            candidatesQuick.Enqueue(thisEdgeData.RouteDuration, edge);
                                        }
                                    }
                                    break;
                            }
                        }
                    } // foreach
                } // of while
            }
            catch(Exception ex)
            {
                Logger.Write(ex.ToString(), TraceEventType.Information, GetType().Name);
                return null;
            }
            
        }

        private bool CheckEndLocation(
            EdgeWithOffset startLocation,
            EdgeWithOffset endLocation,
            RoutingResponse results,
            RouteRequestMultiple request,
            IRoadSpeedCalculator roadSpeedCalculator
            )
        {
            // remove end locations that are the same as the start location. These routes are automatically valid
            // if the index is > start index
            if (startLocation.Edge.RoadLinkEdgeId != endLocation.Edge.RoadLinkEdgeId)
                return true;            // we can add this as a valid target

            // get here if start and end are the same
            if (startLocation.Offset < endLocation.Offset )
            {
                var vec = roadSpeedCalculator.CalculateEdgeCost(request.VehicleType, request.HourOfWeek, startLocation.Edge);
                List<RoadEdgeWithVector> connections = new List<RoadEdgeWithVector>() {
                    new RoadEdgeWithVector
                    {
                        Edge = startLocation.Edge,
                        Vector = vec
                    }                    
                };

                // make a proper linestring
                var path = MakePath(connections, request.StartLocation, endLocation);
                var pathpoints = path.Coordinates.Select(p => new Waypoint { X = p.X, Y = p.Y }).ToArray();

                // add this into the results
                var result = new RoutingResult
                {
                    EndEdge = endLocation,
                    PathPoints = pathpoints,
                    Distance = path.Length,
                    Duration = path.Length / vec.SpeedMs,
                    Connections = connections
                };

                results.Items.Add(result);
            }

            // this route is already processed.. dont add to list of valid targets
            return false;
        }

        private void AddResult(
            IEnumerable<RoutingEndPoint> normalisedEndpoints,
            RoutingEdgeData lastEdge,
            RoutingResponse results,
            RouteRequestMultiple request,
            Dictionary<int, RoutingEdgeData> visited
            )
        {
            // use the original end point if we can find it, to build a proper
            // path to the nearest point on the linestring 
            // we have to make sure that the last edge is included in the list
            var originalEndpoint = normalisedEndpoints.FirstOrDefault(x => Equals(x.Normalised.RoadLinkEdgeId, lastEdge.Edge.RoadLinkEdgeId));

            if (originalEndpoint == null) return;

            foreach (var endLocation in originalEndpoint.Originals)
            {
                // get the edge list
                var connections = MakeRoute(request.StartLocation.Edge, lastEdge.Edge, visited);

                // make a proper linestring
                var path = MakePath(connections, request.StartLocation, endLocation);
                var pathpoints = path.Coordinates.Select(p => new Waypoint { X = p.X, Y = p.Y}).ToArray();

                // add this into the results
                var result = new RoutingResult
                {
                    EndEdge = endLocation,
                    PathPoints = pathpoints,
                    Distance = path.Length,
                    Duration = lastEdge.RouteDuration, // this needs fixing to take into account short start / end sections
                    Connections = connections
                };

                results.Items.Add(result);
            }
        }


        private static LineString MakePath(IEnumerable<RoadEdgeWithVector> connections, EdgeWithOffset startLocation, EdgeWithOffset endLocation)
        {
            var lines = connections.Where(x => x != null).SelectMany(x => x.Edge.Geometry.Coordinates).ToArray();
            var singleLine = new LineString(lines);
            var liL = new LengthIndexedLine(singleLine);
            var startIndex = liL.Project(startLocation.Coord);
            var endIndex = liL.Project(endLocation.Coord);
            var finalLine = liL.ExtractLine(startIndex, endIndex);
            return finalLine as LineString;
        }

        /// <summary>
        ///     Initialise
        /// </summary>
        /// <param name="request"></param>
        /// <param name="startLocation"></param>
        /// <param name="endLocations"></param>
        /// <param name="results"></param>
        /// <param name="candidates"></param>
        /// <param name="candidatesQuick"></param>
        /// <param name="visited"></param>
        private List<RoutingEndPoint> InitialiseRoute(
            IRoadSpeedCalculator speedCalc,
            RouteRequestMultiple request,
            EdgeWithOffset startLocation,
            List<EdgeWithOffset> endLocations,
            RoutingResponse results,
            ref Dictionary<int, RoadEdge> candidates,
            ref FibonacciHeap<double, RoadEdge> candidatesQuick,
            ref Dictionary<int, RoutingEdgeData> visited
            )
        {
            var normalisedEndLocations = new List<RoutingEndPoint>();

            var start = new RoutingEdgeData { Edge = startLocation.Edge, RouteDistance = 0.0, RouteDuration = 0.0};
            visited.Add(start.Edge.RoadLinkEdgeId, start);

            candidates.Add(start.Edge.RoadLinkEdgeId, start.Edge);
            candidatesQuick.Enqueue(0, start.Edge);

            // normalise the end locations

            // the user can pass in locations that have no Edge, i.e. its just an easting/northing.
            // These need to be looked up and converted to a proper RoutingLocation
            if (endLocations != null)
            {
                foreach (var endLocation in endLocations)
                {
                    // check if the end location is the same as the source location .. then
                    // if the end  index > start index then we add the result else reject it
                    var canAdd = CheckEndLocation(startLocation, endLocation, results, request, speedCalc);

                    if (canAdd)
                        // create a new end point record, noting the details of the target edge and the original requested endpoint
                        AddNormalisedEdge(normalisedEndLocations, endLocation);
                }

                foreach (var endLocation in normalisedEndLocations)
                    if (endLocation != null)
                        if (!visited.ContainsKey(endLocation.Normalised.RoadLinkEdgeId))
                        {
                            var end = new RoutingEdgeData {Edge = endLocation.Normalised, IsEndpoint = true};
                            visited.Add(endLocation.Normalised.RoadLinkEdgeId, end);
                        }
            }
            return normalisedEndLocations;
        }

        

        private void AddNormalisedEdge(List<RoutingEndPoint> normalisedEndLocations, EdgeWithOffset original)
        {
            var existing = normalisedEndLocations.FirstOrDefault(x => x.Normalised.RoadLinkEdgeId== original.Edge.RoadLinkEdgeId);
            if (existing == null)
            {
                existing = new RoutingEndPoint
                {
                    //Edge = edge,
                    Normalised = original.Edge,
                    Originals = new List<EdgeWithOffset>() {original}
                };
                normalisedEndLocations.Add(existing);
            }
            else
                existing.Originals.Add(original);

        }

        /// <summary>
        ///     build up a linked list of connections that make up the route from start to end.
        /// </summary>
        /// <param name="startLocation">the routing start location</param>
        /// <param name="stopLocation">the node the routing engine stopped on</param>
        /// <param name="visited">list of visited nodes</param>
        /// <returns></returns>
        private List<RoadEdgeWithVector> MakeRoute(
            RoadEdge startLocation,
            RoadEdge stopLocation,
            Dictionary<int, RoutingEdgeData> visited
            )
        {
            if (visited == null) throw new ArgumentNullException(nameof(visited));
            var route = new List<RoadEdgeWithVector>();

            var currLocation = stopLocation;

            while (currLocation != null && !Equals(currLocation, startLocation))
            {
                var data = visited[currLocation.RoadLinkEdgeId];

                // find the connect that links the previous location with this location
                var conn = data.PreviousEdge;

                RoadEdgeWithVector rewv = new RoadEdgeWithVector { Edge = conn, Vector = data.Vector };
                route.Insert(0, rewv);

                currLocation = data.PreviousEdge;
            }



            // make sure we add the first edge if its not on the list
            var edge = route.First();
            if (edge.Edge != startLocation)
            {
                var data = visited[startLocation.RoadLinkEdgeId];
                RoadEdgeWithVector rewv = new RoadEdgeWithVector { Edge = startLocation, Vector = data.Vector };
                route.Insert(0, rewv);
            }

            // make sure we add the last edge if its not on the list
            var lastEdge = route.Last();
            if (lastEdge.Edge != stopLocation)
            {
                var data = visited[stopLocation.RoadLinkEdgeId];
                RoadEdgeWithVector rewv = new RoadEdgeWithVector { Edge = stopLocation, Vector = data.Vector };
                route.Add(rewv);
            }

            return route;
        }

        /// <summary>
        ///     merges location hits to the target bitmap
        /// </summary>
        /// <param name="c"></param>
        /// <param name="visited"></param>
        public static void ProcessLocations(CoverageMap c, Dictionary<int, RoutingEdgeData> visited)
        {
            // create a data array or 0's and 1's and then merge it with the coverage map.
            var data = new byte[c.Data.Length];

            foreach (var l in visited.Values)
                if (l.Processed)
                    data[c.GetIndex((int) l.Edge.Geometry.EndPoint.X, (int) l.Edge.Geometry.EndPoint.Y)] = 1;

            BufferUtil.Add(data, 0, c.Data, 0, data.Length);
        }

        public RoutingResponse CalculateQuickestRoute(RouteRequest request)
        {
            Logger.Write("Routing Manager: RouteRequestHandler called", TraceEventType.Information, "Routing Manager");

            var start = _data.GetEdgeFromPoint(request.FromLocation);

            var ends = _data.GetEdgesFromPoints(request.ToLocations) ;

            var internalRequest = new RouteRequestMultiple()
            {
                RoadSpeedCalculator = request.RoadSpeedCalculator,
                StartLocation = start,
                EndLocations = ends,
                DistanceMax = double.MaxValue,
                DurationMax = double.MaxValue,
                InstanceMax = 1,
                VehicleType = request.VehicleType,
                SearchType = request.SearchType,
                HourOfWeek = request.HourOfWeek,
                Map = null
            };

            var result = CalculateRouteMultiple(internalRequest);
            return result;
        }
    }
}