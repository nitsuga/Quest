using System;
using System.Collections.Generic;
using Quest.Lib.Routing;
using Quest.Common.Messages;
using GeoAPI.Geometries;
using Quest.Common.Messages;

namespace Quest.Lib.MapMatching.RouteMatcher
{
    public class RouteMatcherResponse
    {
        public int IncidentRouteId;
        public List<RoadLinkEdgeSpeed> Results;
        public Visual Fixes;
        public Visual Route;
        public Visual Particles;

        /// <summary>
        /// Text of the network in GraphVis format. Generated if GenerateGraphVis=true
        /// </summary>
        public string GraphVis;

        public bool IsSuccess;
        public string Message;
    }

    public class RoadLinkEdgeSpeed
    {
        public double RouteDistance;
        public List<RoadEdge> Edges;
        public Waypoint[] PathPoints;
        public double SpeedMs;
        public DateTime StartTime;
        public DateTime EndTime;
        public int Sequence;
        public Coordinate SourceCoord;
        public Coordinate DestCoord;
        public Fix Fix;
        public int Candidates;

        public override string ToString()
        {
            return $"{StartTime} {(int) SpeedMs} ";
        }
    }
}