using System;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class Avls
    {
        public int RawAvlsId { get; set; }
        public DateTime? AvlsDateTime { get; set; }
        public string Status { get; set; }
        public short? Speed { get; set; }
        public short? Direction { get; set; }
        public decimal? LocationX { get; set; }
        public decimal? LocationY { get; set; }
        public short? FleetNumber { get; set; }
        public int? VehicleTypeId { get; set; }
        public string Callsign { get; set; }
        public bool Scanned { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
        public string Category { get; set; }
        public long? IncidentId { get; set; }
        public bool? Process { get; set; }
        public float? EstimatedSpeed { get; set; }
    }
}
