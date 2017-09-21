using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using Quest.Common.Messages;

namespace Quest.Lib.Utils
{
    public static class RouteLine
    {
        public static DbGeometry MakePath(List<RoadEdge> edges)
        {
            var mls = new MultiLineString(edges.Select(x => x.Geometry).ToArray());
            var txt = mls.ToText();
            return DbGeometry.FromText(txt, 27700);
        }
    }
}