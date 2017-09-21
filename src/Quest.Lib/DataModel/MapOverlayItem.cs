using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class MapOverlayItem
    {
        public int MapOverlayItemId { get; set; }
        public int MapOverlayId { get; set; }
        public string Description { get; set; }
        public string FillColour { get; set; }
        public bool Flash { get; set; }
        public bool IsClosed { get; set; }
        public bool Visible { get; set; }
        public byte[] CoverageMap { get; set; }
        public float? AmberLimit { get; set; }
        public float? RedLimit { get; set; }
        public float? FlashLimit { get; set; }
        public string Wkt { get; set; }

        public MapOverlay MapOverlay { get; set; }
    }
}
