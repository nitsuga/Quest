using System;
using System.Collections.Generic;

namespace Quest.Lib.DataModel
{
    public partial class MapOverlay
    {
        public MapOverlay()
        {
            MapOverlayItem = new HashSet<MapOverlayItem>();
        }

        public int MapOverlayId { get; set; }
        public string OverlayName { get; set; }
        public string Stroke { get; set; }
        public int StrokeThickness { get; set; }
        public int FromZoom { get; set; }
        public int ToZoom { get; set; }
        public int GeomUpdateFrequency { get; set; }
        public int AttrUpdateFrequency { get; set; }
        public int? CalculateCoverage { get; set; }

        public ICollection<MapOverlayItem> MapOverlayItem { get; set; }
    }
}
