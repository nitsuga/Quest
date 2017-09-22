﻿using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class Destinations
    {
        public int DestinationId { get; set; }
        public string Destination { get; set; }
        public string Shortcode { get; set; }
        public bool? IsHospital { get; set; }
        public bool? IsStandby { get; set; }
        public bool? IsStation { get; set; }
        public bool? IsRoad { get; set; }
        public bool? IsPolice { get; set; }
        public bool? IsAandE { get; set; }
        public bool? IsOld { get; set; }
        public int? CoverageTier { get; set; }
        public string Status { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Wkt { get; set; }
    }
}