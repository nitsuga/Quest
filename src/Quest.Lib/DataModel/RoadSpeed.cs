﻿namespace Quest.Lib.DataModel
{
    public partial class RoadSpeed
    {
        public int RoadSpeedId { get; set; }
        public double SpeedAvg { get; set; }
        public double? SpeedStDev { get; set; }
        public int SpeedCount { get; set; }
        public int HourOfWeek { get; set; }
        public int VehicleId { get; set; }
        public int? RoadLinkEdgeId { get; set; }
    }
}
