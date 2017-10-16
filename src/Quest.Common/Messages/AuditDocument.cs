using System;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages
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
}