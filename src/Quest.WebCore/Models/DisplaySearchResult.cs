using System.Collections.Generic;
using Nest;
using Quest.Common.Messages.Gazetteer;

namespace Quest.WebCore.Models
{
    public class DisplaySearchResult
    {
        public List<DisplayDocument> Documents;
        public List<Aggregate> Aggregates;
        public long Count;
        public long Removed;
        public long ms;
        public List<List<int>> Grouping { get; set; }
        public PolygonGeoShape Bounds { get; set; }
    }
}
