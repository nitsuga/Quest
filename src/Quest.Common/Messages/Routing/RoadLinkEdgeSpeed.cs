using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using Quest.Common.Messages.GIS;

namespace Quest.Common.Messages.Routing
{
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