using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Quest.Common.Messages
{
    [Serializable]
    public class RoadEdgeWithVector
    {
        public RoadEdge Edge;
        public RoadVector Vector;
    }

    [Serializable]
    public class RoadEdge
    {
        private LineString _geometry;

        public RoadEdge()
        {
            OutEdges = new List<RoadEdge>();
        }

        public RoadEdge(int roadLinkEdgeId, int roadLinkId, string roadName, int roadTypeId, LineString geometry, int sourceGrade, int targetGrade)
        {
            RoadLinkEdgeId = roadLinkEdgeId;
            RoadLinkId = roadLinkId;
            RoadName = roadName;
            RoadTypeId = roadTypeId;
            SourceGrade = sourceGrade;
            TargetGrade = targetGrade;
            Geometry = geometry;
            Length = geometry.Length;
            OutEdges = new List<RoadEdge>();
        }

        [NonSerialized]
        public List<RoadEdge> OutEdges;

        /// <summary>
        /// Level of the start point
        /// </summary>
        
        public int SourceGrade;


        /// <summary>
        /// Level of the end point
        /// </summary>
        public int TargetGrade;

        /// <summary>
        /// name of the road
        /// </summary>
        public string RoadName;

        /// <summary>
        /// the type of road
        /// </summary>
        public int RoadTypeId;

        [JsonIgnore]
        public LineString Geometry
        {
            get
            {
                return _geometry;
            }
            set
            {
                _geometry = value;
                Envelope = value.EnvelopeInternal;
            }
        }

        [JsonIgnore]
        public Envelope Envelope { get; set; }

        /// <summary>
        /// road link Id
        /// </summary>
        public int RoadLinkId { get; set; }
        
        public int RoadLinkEdgeId { get; set; }

        /// <summary>
        /// length of this edge in meters
        /// </summary>
        public double Length;

        public override string ToString()
        {
            return $"{RoadName}";
        }

  }
}