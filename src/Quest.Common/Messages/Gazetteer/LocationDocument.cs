using System;
using System.Collections.Generic;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class LocationDocument : QuestDocument
    {
        [Keyword]
        public string Type { get; set; }

        [Keyword]
        public string Source { get; set; }
        
        //public string ID { get; set; }

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
}