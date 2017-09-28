namespace Quest.Lib.DataModel
{
    public partial class RoadSpeedMatrixDoW
    {
        public int RoadSpeedMatrixDoWid { get; set; }
        public int DayOfWeek { get; set; }
        public float AvgSpeed { get; set; }
        public int VehicleId { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
        public int RoadTypeId { get; set; }
    }
}
