namespace Quest.Lib.Simulation.DataModelSim
{
    public partial class OnsceneStats
    {
        public int CdfId { get; set; }
        public int? CdfType { get; set; }
        public int? Hour { get; set; }
        public string VehicleType { get; set; }
        public string Cdf { get; set; }
        public string Ampds { get; set; }
        public double? Mean { get; set; }
        public double? Stddev { get; set; }
        public int? Count { get; set; }
        public int? VehicleTypeId { get; set; }
    }
}
