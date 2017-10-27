using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Quest.Common.Messages.GIS;

namespace Quest.Common.Messages.Routing
{
    [Serializable]
    public class RouteMatcherResponse
    {
        public string Name;

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

    [Serializable]
    public class RoadLinkEdgeSpeed
    {
        public double RouteDistance;
        public List<RoadEdgeWithVector> Edges;
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