namespace Quest.Lib.OS.DataModelOS
{
    public partial class RoadNetworkMember
    {
        public int RoadNetworkMemberId { get; set; }
        public string RoadFid { get; set; }
        public string NetworkFid { get; set; }
        public int? RoadId { get; set; }
        public int? RoadLinkId { get; set; }

        public Road Road { get; set; }
    }
}
