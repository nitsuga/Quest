using System;
using Nest;

namespace Quest.Lib.Search.Elastic
{
    public class IndexGroup
    {
        internal DateTime ValidFrom;
        internal DateTime ValidTo;

        public int IndexGroupId { get; set; }
        public string Name { get; set; }
        public string Indices { get; set; }
        public bool isEnabled { get; set; }
        public bool isDefault { get; set; }
        public bool useGeometry { get; set; }
        public PolygonGeoShape Polygon { get; set; }
    }
}