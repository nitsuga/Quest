using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class RoadLink
    {
        public RoadLink()
        {
            RoadRouteInfoEndRoadLink = new HashSet<RoadRouteInfo>();
            RoadRouteInfoStartRoadLink = new HashSet<RoadRouteInfo>();
        }

        public int RoadLinkId { get; set; }
        public string Fid { get; set; }
        public string RoadType { get; set; }
        public string Wkt { get; set; }
        public int? FromRoadNodeId { get; set; }
        public int? ToRoadNodeId { get; set; }
        public string FromFid { get; set; }
        public string ToFid { get; set; }
        public string NatureOfRoad { get; set; }
        public bool? Include { get; set; }
        public int? FromGrade { get; set; }
        public int? ToGrade { get; set; }

        public RoadNode FromRoadNode { get; set; }
        public RoadLink RoadLinkNavigation { get; set; }
        public RoadLink InverseRoadLinkNavigation { get; set; }
        public ICollection<RoadRouteInfo> RoadRouteInfoEndRoadLink { get; set; }
        public ICollection<RoadRouteInfo> RoadRouteInfoStartRoadLink { get; set; }
    }
}
