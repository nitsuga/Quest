﻿using System;

namespace Quest.Lib.DataModel
{
    public partial class ResourceArea
    {
        public int AreaId { get; set; }
        public string Area { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public float? Zoom { get; set; }
        public string Wkt { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
