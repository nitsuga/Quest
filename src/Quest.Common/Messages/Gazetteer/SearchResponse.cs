using System;
using System.Collections.Generic;
using Nest;

/// <summary>
/// all of this has moved to the core
/// </summary>
namespace Quest.Common.Messages.Gazetteer
{
    [Serializable]
    public class SearchResponse : Response
    {
        public List<Aggregate> Aggregates;
        public PolygonGeoShape Bounds;
        public long Count;
        public List<SearchHit> Documents;
        public long millisecs;
        public long Removed;
        public List<List<int>> Grouping { get; set; }
    }
}