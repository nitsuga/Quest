using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class RoadNode
    {
        public RoadNode()
        {
            RoadLink = new HashSet<RoadLink>();
            StaticRoadLinksFromRoadNode = new HashSet<StaticRoadLinks>();
            StaticRoadLinksToRoadNode = new HashSet<StaticRoadLinks>();
        }

        public int RoadNodeId { get; set; }
        public string Fid { get; set; }
        public bool? Include { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }

        public ICollection<RoadLink> RoadLink { get; set; }
        public ICollection<StaticRoadLinks> StaticRoadLinksFromRoadNode { get; set; }
        public ICollection<StaticRoadLinks> StaticRoadLinksToRoadNode { get; set; }
    }
}
