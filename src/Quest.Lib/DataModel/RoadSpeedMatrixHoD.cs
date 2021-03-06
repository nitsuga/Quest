﻿namespace Quest.Lib.DataModel
{
    public partial class RoadSpeedMatrixHoD
    {
        public int RoadSpeedMatrixId { get; set; }
        public int HourOfDay { get; set; }
        public float AvgSpeed { get; set; }
        public int VehicleId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int RoadTypeId { get; set; }
    }
}
