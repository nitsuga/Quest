using System.Runtime.Serialization;
using NetTopologySuite.Geometries;
using Quest.Common.Messages;
using Quest.Common.Messages.Routing;

namespace Quest.Lib.OS.Routing.ITN
{
    public class RoadLinkEdgeTemp
    {
        public RoadLinkEdgeTemp(int roadLinkEdgeId, int roadLinkId, string roadName, int roadTypeId, LineString geometry, RoutingLocation to, int sourceGrade, int targetGrade)
        {
            RoadLinkEdgeId = roadLinkEdgeId;
            RoadLinkId = roadLinkId;
            RoadName = roadName;
            RoadTypeId = roadTypeId;
            Target = to;
            SourceGrade = sourceGrade;
            TargetGrade = targetGrade;
            Geometry = geometry;
            Length = geometry.Length;
        }


        /// <summary>
        /// Level of the start point
        /// </summary>
        public int SourceGrade;

        /// <summary>
        /// Level of the end point
        /// </summary>
        public int TargetGrade;

        [DataMember]
        public RoadEdge ReverseEdge { get; set; }

        [DataMember]
        public string RoadName { get; set; }

        [DataMember]
        public int RoadTypeId { get; set; }

        [DataMember]
        public LineString Geometry { get; set; }

        /// <summary>
        /// ITN road link Id
        /// </summary>
        [DataMember]
        public int RoadLinkId { get; set; }

        [DataMember]
        public int RoadLinkEdgeId { get; set; }

        public RoutingLocation Target { get; set; }

        public double Length;

        public override string ToString()
        {
            return $"{RoadName}";
        }

  }
}