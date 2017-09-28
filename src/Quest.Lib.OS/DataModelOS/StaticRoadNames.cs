namespace Quest.Lib.OS.DataModelOS
{
    public partial class StaticRoadNames
    {
        public int RoadNetworkMemberId { get; set; }
        public string RoadName { get; set; }
        public int? ToRoadNodeId { get; set; }
        public int? FromRoadNodeId { get; set; }
        public int RoadLinkId { get; set; }
        public int RoadId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Wkt { get; set; }
    }
}
