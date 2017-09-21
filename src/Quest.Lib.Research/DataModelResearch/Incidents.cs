using System;
using System.Collections.Generic;

namespace Quest.Lib.Research.DataModelResearch
{
    public partial class Incidents
    {
        public DateTime? IncidentDate { get; set; }
        public long Cadref { get; set; }
        public string Dohcategory { get; set; }
        public string Dohsubcat { get; set; }
        public string Lascat { get; set; }
        public DateTime? Callstart { get; set; }
        public int? T0 { get; set; }
        public int? T1 { get; set; }
        public int? T2 { get; set; }
        public int? T3 { get; set; }
        public string Area { get; set; }
        public string Postcode { get; set; }
        public string Complaint { get; set; }
        public string Ampds { get; set; }
        public DateTime? Firstdispatch { get; set; }
        public DateTime? Firstarrival { get; set; }
        public int? Duration { get; set; }
        public DateTime? Athospital { get; set; }
        public string Hospital { get; set; }
        public int? X { get; set; }
        public int? Y { get; set; }
    }
}
