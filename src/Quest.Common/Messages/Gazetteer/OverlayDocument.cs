using System;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    public class OverlayDocument : QuestDocument
    {
        [Text(Index = false)]
        public string Type { get; set; }

        [Text(Index = false)]
        public string Source { get; set; }

        [Text(Index = false)]
        public string ID { get; set; }

        [Text()]
        public string indextext { get; set; }

        [Text(Index = false)]
        public string Description { get; set; }

        [Text(Index = false)]
        public bool Visible { get; set; }

        [Boolean(Index = false)]
        public string Stroke { get; set; }

        [Number(Index = false)]
        public int StrokeThickness { get; set; }

        [Number(Index = false)]
        public int FromZoom { get; set; }

        [Number(Index = false)]
        public int ToZoom { get; set; }

        [Text(Index = false)]
        public string FillColour { get; set; }

        [GeoShape]
        public PolygonGeoShape PolyGeometry { get; set; }

        [GeoShape]
        public MultiLineStringGeoShape LineGeometry { get; set; }
    }
}