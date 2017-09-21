using System;
using System.Collections.Generic;

namespace Quest.Lib.OS.DataModelOS
{
    public partial class Road
    {
        public Road()
        {
            RoadNetworkMember = new HashSet<RoadNetworkMember>();
        }

        public int RoadId { get; set; }
        public string RoadName { get; set; }
        public string Fid { get; set; }

        public ICollection<RoadNetworkMember> RoadNetworkMember { get; set; }
    }
}
