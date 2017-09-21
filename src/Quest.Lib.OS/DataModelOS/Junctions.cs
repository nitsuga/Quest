using System;
using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class Junctions
    {
        public int JunctionId { get; set; }
        public string R1 { get; set; }
        public string R2 { get; set; }
        public int RoadId1 { get; set; }
        public int RoadId2 { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
