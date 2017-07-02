using System.Collections.Generic;
using GeoAPI.Geometries;

namespace Quest.Lib.Utils
{
    public class PolygonData
    {
        public string[] data;
        public IGeometry geom;
    }

    public class IoIPolygonData : PolygonData
    {
        public Dictionary<string, object> attributes;
    }
}