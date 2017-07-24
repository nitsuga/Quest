using System;
using Nest;
using System.Collections.Generic;

namespace Quest.Common.Messages
{
    [Serializable]
    public class IndexGroupRequest : Request
    {
        public bool NotUsed { get; set; }

        public override string ToString()
        {
            return $"IndexGroupRequest";
        }
    }

    [Serializable]
    public class IndexGroupResponse : Response
    {
        public List<IndexGroup> Groups;
        public override string ToString()
        {
            return $"IndexGroupResult {Groups.Count} items";
        }
    }

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