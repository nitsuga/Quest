using System;
using System.Collections.Generic;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [ElasticsearchType(Name = "geofence")]
    public class GeofenceDocument : QuestDocument
    {
        [Text(Index = false)]
        public string TriggerId { get; set; }

        [Text()]
        public string Description { get; set; }

        [Text(Index = false)]
        public string Author { get; set; }

        [Text(Index = false)]
        public string Org { get; set; }

        [Text(Index = false)]
        public string Uprn { get; set; }

        [Text(Index = false)]
        public string Usrn { get; set; }

        [Text(Index = false)]
        public string Notes { get; set; }

        [Text(Index = false)]
        public List<string> Category { get; set; }

        [Date(Index = false)]
        public DateTime? Created { get; set; }

        [Date(Index = false)]
        public DateTime? Review { get; set; }

        [Date(Index = false)]
        public DateTime? ValidFrom { get; set; }

        [Date(Index = false)]
        public DateTime? ValidTo { get; set; }

        [Keyword(Index = false)]
        public string Type { get; set; }

        [GeoShape] 
        public PolygonGeoShape PolyGeometry { get; set; }

        [GeoShape]
        public MultiLineStringGeoShape LineGeometry { get; set; }

        [GeoPoint]
        public PointGeoShape PointGeometry { get; set; } //was geopoint

        [Text(Index = false)]
        public string Claims { get; set; }
    }
}