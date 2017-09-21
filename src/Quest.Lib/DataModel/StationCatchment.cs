using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class StationCatchment
    {
        public int StationCatchmentId { get; set; }
        public string Fid { get; set; }
        public string Code { get; set; }
        public string StationName { get; set; }
        public string Complex { get; set; }
        public string Area { get; set; }
        public int? ComplexId { get; set; }
        public bool? Enabled { get; set; }
        public string Wkt { get; set; }
    }
}
