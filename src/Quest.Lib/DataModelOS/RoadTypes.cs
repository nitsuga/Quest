using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModelOS
{
    public partial class RoadTypes
    {
        public RoadTypes()
        {
            StaticRoadLinks = new HashSet<StaticRoadLinks>();
        }

        public int RoadTypeId { get; set; }
        public string RoadType { get; set; }

        public ICollection<StaticRoadLinks> StaticRoadLinks { get; set; }
    }
}
