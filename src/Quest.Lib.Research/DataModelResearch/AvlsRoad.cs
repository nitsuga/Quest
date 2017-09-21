using System;
using System.Collections.Generic;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class AvlsRoad
    {
        public int AvlsRoadId { get; set; }
        public int AvlsId { get; set; }
        public int RoadLinkEdgeId { get; set; }
        public int RoadTypeId { get; set; }
        public float DistanceToRoad { get; set; }
    }
}
