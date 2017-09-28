namespace Quest.Lib.OS.DataModelOS
{
    public partial class RoadRouteInfo
    {
        public int RoadRouteInfoId { get; set; }
        public string Fid { get; set; }
        public int? StartRoadLinkId { get; set; }
        public int? EndRoadLinkId { get; set; }
        public string Instruction { get; set; }
        public string FromFid { get; set; }
        public string ToFid { get; set; }

        public RoadLink EndRoadLink { get; set; }
        public RoadLink StartRoadLink { get; set; }
    }
}
