using System;

namespace Quest.Lib.DataModel
{
    public partial class Call
    {
        public int CallId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string Address4 { get; set; }
        public string Address5 { get; set; }
        public string Address6 { get; set; }
        public int? Easting { get; set; }
        public int? Northing { get; set; }
        public int? SemiMajor { get; set; }
        public int? SemiMinor { get; set; }
        public string Name { get; set; }
        public int? Confidence { get; set; }
        public int? Angle { get; set; }
        public int? Altitude { get; set; }
        public int? Direction { get; set; }
        public int? Speed { get; set; }
        public DateTime? Updated { get; set; }
        public long? SwitchId { get; set; }
        public string Status { get; set; }
        public int? Requery { get; set; }
        public string Extension { get; set; }
        public string Event { get; set; }
        public bool? IsMobile { get; set; }
        public bool? IsClosed { get; set; }
        public DateTime? TimeConnected { get; set; }
        public DateTime? TimeAnswered { get; set; }
        public DateTime? TimeClosed { get; set; }
    }
}
