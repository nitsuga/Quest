using System;
using Nest;

namespace Quest.Common.Messages.Gazetteer
{

    [Serializable]
    public class IndexGroup
    {
        public DateTime ValidFrom;
        public DateTime ValidTo;
        public int IndexGroupId;
        public string Name;
        public string Indices;
        public bool isEnabled;
        public bool isDefault;
        public bool useGeometry;

        [NonSerialized]
        public PolygonGeoShape Polygon;
        
        public string PolygonWkt;

        public override string ToString()
        {
            return $"IndexGroup {Name}";
        }
    }
}