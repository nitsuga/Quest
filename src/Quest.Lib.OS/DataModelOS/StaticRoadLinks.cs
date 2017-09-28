namespace Quest.Lib.OS.DataModelOS
{
    public partial class StaticRoadLinks
    {
        public int RoadLinkId { get; set; }
        public int? FromRoadNodeId { get; set; }
        public int? ToRoadNodeId { get; set; }
        public string RoadName { get; set; }
        public int RoadTypeId { get; set; }
        public string Wkt { get; set; }
        public bool? FromOneWay { get; set; }
        public bool? FromNoTurn { get; set; }
        public bool? FromNoEntry { get; set; }
        public bool? FromMandatoryTurn { get; set; }
        public bool? ToOneWay { get; set; }
        public bool? ToNoTurn { get; set; }
        public bool? ToNoEntry { get; set; }
        public bool? ToMandatoryTurn { get; set; }
        public int FromGrade { get; set; }
        public int ToGrade { get; set; }

        public RoadNode FromRoadNode { get; set; }
        public RoadTypes RoadType { get; set; }
        public RoadNode ToRoadNode { get; set; }
    }
}
