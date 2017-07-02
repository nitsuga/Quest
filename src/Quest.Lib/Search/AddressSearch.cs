using System;
using System.Collections.Generic;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Lib.Search
{
    [Serializable, ElasticsearchType(Name = "AuditDocument")]
    public class AuditDocument
    {
        [Date]
        public DateTime timestamp { get; set; }

        [Number(NumberType.Long)]
        public long duration { get; set; }

        [Text]
        public string type { get; set; }

        [Text(Index = false)]
        public string user { get; set; }

        [Text]
        public string description { get; set; }

        [GeoShape] //Index = FieldIndexOption.Analyzed
        public PolygonGeoShape poly { get; set; }
    }

    public enum SearchMode
    {
        EXACT,
        RELAXED,
        FUZZY
    }

    [Serializable, ElasticsearchType(Name = "SearchHistoryDocument")]
    public class GazSearchRequest
    {
        [Date]
        public DateTime timestamp { get; set; }

        [Text]
        public string indexGroup { get; set; }

        [Text]
        public string searchText { get; set; }

        [Boolean(Index = false)]
        public SearchMode searchMode { get; set; }

        [Boolean(Index = false)]
        public bool includeAggregates { get; set; }

        [Number(NumberType.Integer, Index = false)]
        public int skip { get; set; }

        [Number(NumberType.Integer, Index = false)]
        public int take { get; set; }

        [Object(Enabled = false)]
        public DistanceFilter distance { get; set; }

        public BBFilter box { get; set; }

        public PolygonGeoShape polygon { get; set; } // filter within this geometry

        public List<TermFilter> filters { get; set; }

        /// <summary>
        /// specifies locating information documents whose geofence contains this point
        /// </summary>
        public GeoCoordinate infopoint { get; set; }

        [Object(Ignore = true)]
        public SearchResultDisplayGroup displayGroup { get; set; }

        [Date]
        public DateTime searchTime { get; set; }

        [Text(Index = false)]
        public string username { get; set; }
    }

    public enum SearchResultDisplayGroup
    {
        none = 1,
        description = 2,
        thoroughfare = 3,
        type = 4
    }

    [Serializable]
    public class SearchResult
    {
        public List<Aggregate> Aggregates;
        public PolygonGeoShape Bounds;
        public long Count;
        public List<SearchHit> Documents;
        public long millisecs;
        public long Removed;
        public List<List<int>> Grouping { get; set; }
    }

    public class GroupKey
    {
        public string Name { get; set; }
        public double Score { get; set; }
    }


    [Serializable]
    public class SearchHit
    {
        public AddressDocument l;
        public double s;

        public override string ToString()
        {
            return l.indextext;
        }
    }


    [Serializable]
    public class TermFilter
    {
        public string field;
        public bool include;
        public string value;
    }

    [Serializable]
    public class DistanceFilter
    {
        public string distance;
        public double lat;
        public double lng;
    }

    [Serializable]
    public class BBFilter
    {
        public double br_lat;
        public double br_lon;
        public double tl_lat;
        public double tl_lon;
    }


    [Serializable]
    public class Aggregate
    {
        public List<AggregateItem> Items;
        public string Name { get; set; }
    }

    [Serializable]
    public class AggregateItem
    {
        public string Name { get; set; }
        public long? Value { get; set; }
    }

    [Serializable]
    public class InfoSearchRequest
    {
        public double lat;
        public double lng;
    }


    [Serializable]
    public class InfoSearchResult
    {
        public long Count;
        public List<AddressDocument> Documents;
    }


    [Serializable]
    public class Location
    {
        public double[] Coordinate; // centroid or point
        public string Precision = "1m";
        public string Radius;
        public string Type;
        public string WKT;


        public override string ToString()
        {
            return $"{Coordinate[0]} {Coordinate[1]}";
        }
    }

    //[Serializable]
    //public class GeoPoint
    //{
    //    //public string type;
    //    public double lat;
    //    public double lon;

    //    public override string ToString()
    //    {
    //        return string.Format("{0} {1}", lat, lon);
    //    }
    //}


    public class QuestDocument
    {
    }

    [ElasticsearchType(Name = "geofence")]
    public class GeofenceDocument : QuestDocument
    {
        [Text(Index = false)]
        public string Id { get; set; }

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

    public class AddressDocument : QuestDocument
    {
        [Keyword]
        public string Type { get; set; }

        [Keyword]
        public string Source { get; set; }
        
        public string ID { get; set; }

        [Text]
        public string indextext { get; set; }

        [Text(Index = false)]
        public string Description { get; set; }

        public List<string> Thoroughfare { get; set; }

        public List<string> Locality { get; set; }

        public string[] Areas { get; set; }

        [Keyword]
        public string Roadtype { get; set; }

        [Date(Index = false)]
        public DateTime Created { get; set; }

        [Number(Index = false)]
        public long UPRN { get; set; }

        [Number(Index = false)]
        public long USRN { get; set; }

        [Keyword(Index = false)]
        public string Status { get; set; }

        public GeoLocation Location { get; set; }

        public PointGeoShape Point { get; set; }

        [GeoShape]
        public PolygonGeoShape Poly { get; set; }


        [GeoShape]
        public MultiLineStringGeoShape MultiLine { get; set; }

        [Keyword(Index = false)]
        public string GroupingIdentity { get; set; }

        [Keyword(Index = false)]
        public string Classification { get; set; }

        /// <summary>
        /// URL of this information
        /// </summary>
        [Text(Index = false)]
        public string Url { get; set; }

        /// <summary>
        /// image content
        /// </summary>
        [Text(Index = false)]
        public string Image { get; set; }

        /// <summary>
        /// video content
        /// </summary>
        [Text(Index = false)]
        public string Video { get; set; }

        /// <summary>
        /// HTML content for this item
        /// </summary>
        [Text(Index = false)]
        public string Content { get; set; }

        /// <summary>
        /// Specify a trigger footprint for information
        /// </summary>
        [GeoShape]
        public PolygonGeoShape InfoGeofence { get; set; }

        [Text(Index = false)]
        public string InfoClasses { get; set; }
    }

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