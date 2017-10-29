using System.Collections.Generic;

namespace Quest.Common.Messages.Visual
{
    /// <summary>
    /// A route visual containing a list of road segments.
    /// Each segment contains a speed, polyline and road name
    /// </summary>
    public class VisualRoute
    {
        public List<Segment> Segments;
        
        public class Segment
        {
            public string Wkt { get; set; }
            public string Roadname { get; set; }
            public int Speed { get; set; }
        }
    }


}