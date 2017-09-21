using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class RoadLinkEdgeLink
    {
        public int RoadLinkEdgeLinkId { get; set; }
        public int SourceRoadLinkEdge { get; set; }
        public int TargetRoadLinkEdge { get; set; }
    }
}
