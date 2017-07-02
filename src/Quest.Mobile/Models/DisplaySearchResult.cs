#pragma warning disable 0169,649
using System.Collections.Generic;
#if NET45
using System.Data.Spatial;
#endif
using Quest.Common.Messages;
using Nest;

namespace Quest.Mobile.Models
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
