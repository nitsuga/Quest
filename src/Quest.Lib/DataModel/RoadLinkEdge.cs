using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class RoadLinkEdge
    {
        public int RoadLinkEdgeId { get; set; }
        public int RoadLinkId { get; set; }
        public string RoadName { get; set; }
        public int RoadTypeId { get; set; }
        public int SourceGrade { get; set; }
        public int TargetGrade { get; set; }
        public int Length { get; set; }
        public string Wkt { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
