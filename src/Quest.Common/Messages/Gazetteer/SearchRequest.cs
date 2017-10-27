using System;
using System.Collections.Generic;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable, ElasticsearchType(Name = "SearchHistoryDocument")]
    public class SearchRequest : Request
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

        [NonSerialized]
        public PolygonGeoShape polygon;

        public List<TermFilter> filters { get; set; }

        /// <summary>
        /// specifies locating information documents whose geofence contains this point
        /// </summary>
        [NonSerialized]
        public GeoCoordinate infopoint;

        [Object(Ignore = true)]
        public SearchResultDisplayGroup displayGroup { get; set; }

        [Date]
        public DateTime searchTime { get; set; }

        [Text(Index = false)]
        public string username { get; set; }
    }
}